using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Models;
using TeamCubing.DAL.Models;

namespace TeamCubing.API.Hubs;

[Authorize]
public class RoomHub : Hub
{
    private readonly ILogger<RoomHub> _logger;
    private readonly IMapper _mapper;
    private readonly IRoomService _roomService;
    private readonly ApplicationUser _user;
    private readonly IUserService _userService;

    public RoomHub(
        ApplicationUser user,
        IUserService userService,
        IRoomService roomService,
        ILogger<RoomHub> logger,
        IMapper mapper)
    {
        _user = user;
        _userService = userService;
        _roomService = roomService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task Send(int roomId, int solveId, int timeMilliseconds)
    {
        var room = await _roomService.GetRoomWithUsersAndSolvesAsync(roomId);

        if (room.ConnectedUserNames.Contains(_user.UserName))
        {
            var userId = await _userService.GetUserIdAsync(_user.UserName);
            var result = new RoomSolveResult
            {
                UserId = userId,
                Time = timeMilliseconds,
                RoomSolveId = solveId,
            };

            var response = await _roomService.AddUserResultAsync(result);
            var serializedResult = JsonSerializer.Serialize(
                result,
                new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                });

            _logger.LogInformation("New user result added {Result}", serializedResult);

            await Clients.Group(room.Name)
                .SendAsync(
                    "Send",
                    response);

            if (await _roomService.IsSolveFinished(solveId))
            {
                await SendNewSolveNotificationAsync(room);
            }
        }
    }

    public async Task JoinGroup(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("NewUser", _user.UserName);

        _logger.LogInformation("User {UserName} joined room {RoomName}", _user.UserName, roomName);
    }

    public async Task LeaveGroup(string roomName)
    {
        await _roomService.LeaveCurrentRoomAsync();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("UserLeft", _user.UserName);
    }

    public async Task NewSolve(int roomId)
    {
        var room = await _roomService.GetRoomWithUsersAndSolvesAsync(roomId);

        var lastSolve = room.Solves.OrderByDescending(r => r.SolveNumber).First();

        if (await _roomService.IsSolveFinished(lastSolve.Id))
        {
            await SendNewSolveNotificationAsync(room);
        }
    }

    public async Task ForceNewSolve(int roomId)
    {
        var room = await _roomService.GetRoomWithUsersAndSolvesAsync(roomId);

        await SendNewSolveNotificationAsync(room);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var oldRoomName = await _roomService.LeaveCurrentRoomAsync();

        if (!string.IsNullOrEmpty(oldRoomName))
        {
            await Clients.Group(oldRoomName).SendAsync("UserLeft", _user.UserName);
        }

        _logger.LogInformation("User {UserName} left room {RoomName}", _user.UserName, oldRoomName);

        await base.OnDisconnectedAsync(exception);
    }

    private async Task SendNewSolveNotificationAsync(RoomDto room)
    {
        var nextSolve = await _roomService.CreateNextSolveInRoomAsync(room);
        await Clients.Group(room.Name).SendAsync("SolveFinished", nextSolve);
    }
}
