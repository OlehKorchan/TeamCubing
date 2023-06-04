using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TeamCubing.API.Hubs;
using TeamCubing.API.Models;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Models;

namespace TeamCubing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly ILogger<RoomsController> _logger;
    private readonly IMapper _mapper;
    private readonly IHubContext<RoomHub> _roomHub;
    private readonly IRoomService _roomService;

    public RoomsController(
        IHubContext<RoomHub> roomHub,
        IRoomService roomService,
        IMapper mapper,
        ILogger<RoomsController> logger)
    {
        _roomHub = roomHub;
        _roomService = roomService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        return Ok(await _roomService.GetAllAsync());
    }

    [HttpGet("leaveCurrentRoom")]
    public async Task<IActionResult> LeaveRoomAsync()
    {
        return Ok(await _roomService.LeaveCurrentRoomAsync());
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
    public async Task<IActionResult> CreateAsync(RoomLoginModel loginModel)
    {
        var result = await _roomService.CreateRoomAsync(
            _mapper.Map<RoomLoginModel, RoomLoginDto>(loginModel));

        if (result is null)
        {
            return Ok(false);
        }

        var serializedResult = JsonSerializer.Serialize(
            result,
            new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });

        _logger.LogInformation("Room Created: {SerializedResult}", serializedResult);

        return Ok(true);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginToRoomAsync(RoomLoginModel loginModel)
    {
        var result =
            await _roomService.LoginToRoomAsync(
                _mapper.Map<RoomLoginModel, RoomLoginDto>(loginModel));

        return Ok(result);
    }
}
