using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TeamCubing.API.Hubs;
using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.DTO;
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

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        return Ok(await _roomService.GetAllRoomsDataAsync());
    }

    [HttpGet("kick/{userName}")]
    public async Task<IActionResult> LeaveRoomAsync(string userName)
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
    public async Task<IActionResult> CheckAccessAsync(string roomName)
    {
        var result = await _roomService.CheckAccessAsync(roomName);
        return result switch
        {
            RoomCheckAccessResult.Authorized => Ok(true),
            RoomCheckAccessResult.Forbidden => Ok(false),
            RoomCheckAccessResult.NotFound => NotFound(),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] RoomCreateRequest request)
    {
        var result = await _roomService.CreateRoomAsync(request);

        return Ok(result);
    }

    [HttpDelete("{roomId}")]
    public async Task<IActionResult> DeleteAsync(string roomId)
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
    public async Task<IActionResult> LoginToRoomAsync(RoomLoginRequest request)
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