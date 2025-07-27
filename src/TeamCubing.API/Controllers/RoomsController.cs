using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TeamCubing.API.Hubs;
using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;
using TeamCubing.Domain.ResponseModels;

namespace TeamCubing.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly IHubContext<RoomHub> _roomHub;
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService, IHubContext<RoomHub> roomHub)
    {
        _roomService = roomService;
        _roomHub = roomHub;
    }

    /// <summary>
    /// Returns all existing rooms
    /// </summary>
    /// <returns>List of all existing rooms</returns>
    [HttpGet]
    [ProducesResponseType<List<RoomDisplayDataResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoomDisplayDataResponse>>> GetAllRooms()
    {
        return Ok(await _roomService.GetAllRoomsDataAsync());
    }

    [HttpGet("kick/{userName}")]
    public async Task<IActionResult> LeaveRoom(string userName)
    {
        var leftRoomName = await _roomService.LeaveLastRoomAsync(userName);

        if (string.IsNullOrEmpty(leftRoomName))
        {
            return NotFound();
        }

        await _roomHub.Clients.Group(leftRoomName).SendAsync("UserLeft", userName);

        return Ok();

    }

    [HttpGet("checkAccess/{roomName}")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> CheckAccess(string roomName)
    {
        var result = await _roomService.CheckAccessAsync(roomName);
        return result switch
        {
            RoomCheckAccessResult.Authorized => Ok(true),
            RoomCheckAccessResult.Forbidden => Ok(false),
            RoomCheckAccessResult.NotFound => NotFound(),
            _ => BadRequest()
        };
    }

    [HttpPost]
    [ProducesResponseType<ModelResponse<Room>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ModelResponse<Room>>> CreateRoom([FromBody] RoomCreateRequest request)
    {
        var result = await _roomService.CreateRoomAsync(request);

        return Ok(result);
    }

    [HttpDelete("{roomId}")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RoomOperationResponse<Room>>> DeleteRoom(string roomId)
    {
        var result = await _roomService.RemoveRoomAsync(roomId);

        if (result.IsSuccess)
        {
            await _roomHub.Clients.Group(result.RoomName).SendAsync("Removed");

            return Ok(true);
        }

        return Ok(false);
    }

    [HttpPost("login")]
    [ProducesResponseType<RoomLoginResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RoomLoginResponse>> LoginToRoom(RoomLoginRequest request)
    {
        var result = await _roomService.LoginToRoomAsync(request);

        return Ok(result);
    }

    [HttpDelete("{roomName}/solves/{solveNumber:int}")]
    public async Task<IActionResult> RemoveSolve(
        [FromRoute] string roomName,
        [FromRoute] int solveNumber)
    {
        var result = await _roomService.RemoveSolveAsync(new RemoveSolveRequest
        {
            RoomName = roomName,
            SolveNumber = solveNumber
        });

        if (result.IsSuccess)
        {
            await _roomHub.Clients.Group(roomName).SendAsync("ResultRemove", solveNumber);
            return Ok(true);
        }

        if (result.StatusCode is HttpStatusCode.Forbidden)
        {
            return new ForbidResult();
        }

        return Ok(false);
    }

    [HttpDelete("{roomName}/solves/{solveNumber:int}/user/{userName}/result")]
    public async Task<IActionResult> RemoveUserResult(
        [FromRoute] string roomName,
        [FromRoute] int solveNumber,
        [FromRoute] string userName)
    {
        var result = await _roomService.RemoveUserResultFromRoom(new RemoveUserResultRequest
        {
            RoomName = roomName,
            SolveNumber = solveNumber,
            UserName = userName
        });

        if (result.IsSuccess)
        {
            await _roomHub.Clients.Group(roomName).SendAsync("ResultRemove", new UserResultRemoveResponseModel
            {
                SolveNumber = solveNumber,
                UserName = userName
            });
            return Ok(true);
        }

        if (result.StatusCode is HttpStatusCode.Forbidden)
        {
            return new ForbidResult();
        }

        return Ok(false);
    }
}