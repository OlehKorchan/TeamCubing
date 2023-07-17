using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TeamCubing.BLL.Helpers.Extensions;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Models;
using TeamCubing.DAL.Interfaces;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Extensions;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;
using TeamCubing.Domain.ResponseModels;

namespace TeamCubing.BLL.Services;

public class RoomService : IRoomService
{
    private const int MinCachedScrambles = 5;
    private const int MaxCachedScrambles = 10;
    private const int RoomSolveMaxDurationSeconds = 240;
    private readonly ILogger<RoomService> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly IScramblerService _scramblerService;
    private readonly ClaimsPrincipal _user;
    private readonly IUserRepository _userRepository;

    public RoomService(
        IRoomRepository roomRepository,
        ClaimsPrincipal user,
        ILogger<RoomService> logger,
        IScramblerService scramblerService,
        IUserRepository userRepository)
    {
        _roomRepository = roomRepository;
        _user = user;
        _logger = logger;
        _scramblerService = scramblerService;
        _userRepository = userRepository;
    }

    public async Task<RoomCheckAccessResult> CheckAccessAsync(string roomName)
    {
        var room = await _roomRepository.ReadByNameAsync(roomName);

        if (room == null)
        {
            return RoomCheckAccessResult.NotFound;
        }

        return room.Settings.IsOpen ||
               room.ConnectedUserNames.Contains(_user.UserName()) ||
               room.WasOnceConnectedUserNames.Contains(_user.UserName())
            ? RoomCheckAccessResult.Authorized
            : RoomCheckAccessResult.Forbidden;
    }

    public async Task<ModelResponse<Room>> CreateRoomAsync(RoomCreateRequest request)
    {
        var result = new ModelResponse<Room>();
        if (string.IsNullOrEmpty(request.RoomName))
        {
            // TODO: Move all hardcoded strings to messages class
            result.ErrorMessage = "Invalid room name";

            return result;
        }

        request.Settings ??= new RoomSettings
        {
            IsOpen = false,
            UsersLimit = 3,
            EnableSolveTimeLimit = true,
        };

        if (!request.Settings.IsOpen && request.RoomPassword is null)
        {
            result.ErrorMessage = "Invalid room password";

            return result;
        }

        var isRoomExist = await _roomRepository.ReadByNameAsync(request.RoomName) != null;

        if (isRoomExist)
        {
            result.ErrorMessage = "Room with provided name already exists";

            return result;
        }

        var room = await _roomRepository.InsertAsync(
            new Room
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.RoomName,
                Password = request.RoomPassword,
                Settings = request.Settings,
                WasOnceConnectedUserNames = new List<string>
                {
                    _user.UserName(),
                },
            });

        _logger.LogInformation("Room Created: {SerializedResult}", room.ToJsonString());

        result.IsSuccess = true;
        result.Model = room;

        return result;
    }

    public async Task<SolvePushResponse> PushSolveToRoomAsync(string roomId, bool isForce)
    {
        var result = new SolvePushResponse();

        var room = await _roomRepository.ReadByIdAsync(roomId);

        if (room is null)
        {
            return result;
        }

        result.RoomName = room.Name;

        var lastSolve = GetLastSolveInRoom(room);

        if (lastSolve is null || IsSolveFinished(lastSolve, room) || isForce)
        {
            var created = await CreateNextSolveInRoomAsync(room);

            if (created is null)
            {
                return result;
            }

            result.IsSuccess = true;
            result.Model = created;
        }

        return result;
    }

    public async Task<List<RoomDisplayDataResponse>> GetAllRoomsDataAsync()
    {
        var allRooms = await _roomRepository.ReadAllAsync();

        return allRooms
            .Select(
                r => new RoomDisplayDataResponse
                {
                    IsOpen = r.Settings.IsOpen,
                    RoomName = r.Name,
                    Puzzle = r.Settings.Puzzle,
                    ConnectedUsersCount = r.ConnectedUserNames.Count,
                    MaxUsersCount = r.Settings.UsersLimit,
                })
            .ToList();
    }

    public async Task<RoomLoginResponse> LoginToRoomAsync(RoomLoginRequest request)
    {
        var result = new RoomLoginResponse();
        var (validationResult, room) = await PerformRoomValidationAsync(request);

        if (!validationResult ||
            IsLoginForbidden(room, request.RoomPassword, out var isUserNeverJoined))
        {
            return result;
        }

        if (isUserNeverJoined)
        {
            room.WasOnceConnectedUserNames.Add(_user.UserName());
        }

        var isUserNotInRoom = room.ConnectedUserNames.All(u => u != _user.UserName());

        if (isUserNotInRoom)
        {
            room.ConnectedUserNames.Add(_user.UserName());
        }

        if (isUserNeverJoined || isUserNotInRoom)
        {
            await _roomRepository.ReplaceAsync(room);
        }

        await UpdateLastUserRoomAsync(room.Name);

        result.IsSuccess = true;
        result.Model = room;

        return result;
    }

    public async Task<RoomOperationResponse<SolveResult>> AddUserResultAsync(
        NewUserResultRequest request)
    {
        var methodResult = new RoomOperationResponse<SolveResult>();
        var room = await _roomRepository.ReadByIdAsync(request.RoomId);

        if (room is not null && IsUserInRoom(room))
        {
            var roomSolve = room.Solves.FirstOrDefault(s => s.SolveNumber == request.SolveNumber);

            if (roomSolve is not null && IsFirstUserResult(roomSolve))
            {
                var newResult = new SolveResult
                {
                    UserName = _user.UserName(),
                    Time = request.TimeInMilliseconds,
                    Penalty = request.Penalty,
                };

                await _roomRepository.PatchUserResults(
                    room.Name,
                    room.Solves.IndexOf(roomSolve),
                    newResult);

                // roomSolve.Results.Add(newResult);
                //
                // await _roomRepository.ReplaceAsync(room);

                _logger.LogInformation(
                    "New user result added {Result}",
                    newResult.ToJsonString());

                methodResult.IsSuccess = true;
                methodResult.RoomName = room.Name;
                methodResult.Model = newResult;
            }
        }

        return methodResult;
    }

    public async Task<string> LeaveLastRoomAsync()
    {
        var lastRoomName = (await _userRepository.ReadByNameAsync(_user.UserName())).LastRoomName;

        if (string.IsNullOrEmpty(lastRoomName))
        {
            return lastRoomName;
        }

        var lastRoom = await _roomRepository.ReadByNameAsync(lastRoomName);
        lastRoom.ConnectedUserNames.Remove(_user.UserName());

        await _roomRepository.ReplaceAsync(lastRoom);

        return lastRoomName;
    }

    private async Task<Solve> CreateNextSolveInRoomAsync(Room room)
    {
        var nextSolveNumber = room.Solves.Count + 1;

        var scrambleWithImage = GetScrambleFromCacheOrGenerate(room);

        var solve = new Solve
        {
            SolveNumber = nextSolveNumber,
            Scramble = scrambleWithImage.Scramble,
            StartTime = DateTime.UtcNow,
            ScrambledPuzzleImage = scrambleWithImage.Image,
        };

        room.Solves.Add(solve);

        await _roomRepository.ReplaceAsync(room);

        _logger.LogInformation(
            "Created solve: {Solve} in room: {RoomName}",
            solve.ToJsonString(),
            room.Name);

        return solve;
    }

    private static bool IsSolveFinished(Solve solve, Room room)
    {
        return room.ConnectedUserNames.All(un => solve.Results.Any(r => r.UserName == un)) ||
               (room.Settings.EnableSolveTimeLimit &&
                solve.StartTime.AddSeconds(RoomSolveMaxDurationSeconds) <= DateTime.UtcNow);
    }

    private async Task<(bool, Room)> PerformRoomValidationAsync(RoomLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.RoomName))
        {
            return (false, null);
        }

        var room = await _roomRepository.ReadByNameAsync(request.RoomName);

        return room == null ? (false, null) : (true, room);
    }

    private bool IsLoginForbidden(
        Room room,
        string providedPassword,
        out bool isUserNeverJoined)
    {
        isUserNeverJoined = room.WasOnceConnectedUserNames.All(u => u != _user.UserName());

        if ((!room.Settings.IsOpen && isUserNeverJoined && room.Password != providedPassword) ||
            room.ConnectedUserNames.Count >= room.Settings.UsersLimit)
        {
            return true;
        }

        return false;
    }

    private static Solve GetLastSolveInRoom(Room room)
    {
        if (room.Solves.Any())
        {
            return room.Solves.OrderByDescending(r => r.SolveNumber).First();
        }

        return null;
    }

    private bool IsFirstUserResult(Solve solve)
    {
        return solve.Results.All(r => r.UserName != _user.UserName());
    }

    private bool IsUserInRoom(Room room)
    {
        return room.ConnectedUserNames.Any(r => r == _user.UserName());
    }

    private async Task UpdateLastUserRoomAsync(string roomName)
    {
        var user = await _userRepository.ReadByNameAsync(_user.UserName());

        user.LastRoomName = roomName;

        await _userRepository.ReplaceAsync(user);
    }

    private ScrambleWithImage GetScrambleFromCacheOrGenerate(Room room)
    {
        var result = room.CachedScrambles.LastOrDefault();

        if (result is null)
        {
            result = _scramblerService.GenerateScrambleWithImage(room.Settings.Puzzle);

            Task.Run(() => UpdateCachedScrambles(room));
        }
        else
        {
            room.CachedScrambles.Remove(result);

            if (room.CachedScrambles.Count < MinCachedScrambles)
            {
                Task.Run(() => UpdateCachedScrambles(room));
            }
        }

        return result;
    }

    private async Task UpdateCachedScrambles(Room room)
    {
        for (var i = 0; i < MaxCachedScrambles - room.CachedScrambles.Count; i++)
        {
            room.CachedScrambles.Add(_scramblerService.GenerateScrambleWithImage(room.Settings.Puzzle));
        }

        await _roomRepository.PatchScrambleCache(room.Name, room.CachedScrambles);
    }
}
