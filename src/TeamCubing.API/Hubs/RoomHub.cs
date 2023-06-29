using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Models;
using TeamCubing.Domain.Extensions;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;

namespace TeamCubing.API.Hubs;

[Authorize]
public class RoomHub : Hub
{
    private readonly ILogger<RoomHub> _logger;
    private readonly IRoomService _roomService;
    private readonly ClaimsPrincipal _user;

    public RoomHub(
        ClaimsPrincipal user,
        IRoomService roomService,
        ILogger<RoomHub> logger)
    {
        _user = user;
        _roomService = roomService;
        _logger = logger;
    }

    public async Task NewResult(
        string roomId,
        int solveNumber,
        int timeMilliseconds,
        Penalty penalty)
    {
        var response = await _roomService.AddUserResultAsync(
            new NewUserResultRequest
            {
                RoomId = roomId,
                SolveNumber = solveNumber,
                TimeInMilliseconds = timeMilliseconds,
                Penalty = penalty,
            });

        if (response.IsSuccess)
        {
            await Clients.Group(response.RoomName).SendAsync(nameof(NewResult), response.Model);

            await AskForNewSolve(roomId);
        }
    }

    public async Task JoinGroup(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("NewUser", _user.UserName());

        _logger.LogInformation(
            "User {UserName} joined room {RoomName}",
            _user.UserName(),
            roomName);
    }

    public async Task LeaveGroup(string roomName)
    {
        try
        {
            await _roomService.LeaveLastRoomAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                "Logout failed for room: {RoomName}, with exception: {Message}",
                roomName,
                e.Message);
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("UserLeft", _user.UserName());
    }

    public async Task AskForNewSolve(string roomId)
    {
        var result = await _roomService.PushSolveToRoomAsync(roomId, false);

        await ProcessPushSolveResultAsync(result);
    }

    public async Task ForceNewSolve(string roomId)
    {
        var result = await _roomService.PushSolveToRoomAsync(roomId, true);

        await ProcessPushSolveResultAsync(result);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var room = await _roomService.LeaveLastRoomAsync();

        if (!string.IsNullOrEmpty(room))
        {
            await Clients.Group(room).SendAsync("UserLeft", _user.UserName());
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task ProcessPushSolveResultAsync(SolvePushResponse response)
    {
        if (response.IsSuccess)
        {
            await Clients.Group(response.RoomName).SendAsync("SolveFinished", response.Model);
        }
    }
}
