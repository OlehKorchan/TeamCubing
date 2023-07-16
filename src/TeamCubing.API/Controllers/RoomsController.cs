using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TeamCubing.API.Hubs;
using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.RequestModels;

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

    [HttpGet("leaveCurrentRoom")]
    public async Task<IActionResult> LeaveRoomAsync()
    {
        return Ok(await _roomService.LeaveLastRoomAsync());
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
}
