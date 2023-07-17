using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TeamCubing.BLL.Helpers.Extensions;
using TeamCubing.BLL.Interfaces;
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
    private readonly IMapper _mapper;

    public RoomService(
        IRoomRepository roomRepository,
        ClaimsPrincipal user,
        ILogger<RoomService> logger,
        IScramblerService scramblerService,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _roomRepository = roomRepository;
        _user = user;
        _logger = logger;
        _scramblerService = scramblerService;
        _userRepository = userRepository;
        _mapper = mapper;
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
                Id = request.RoomName,
                Name = request.RoomName,
                Password = request.RoomPassword,
                Settings = request.Settings,
                WasOnceConnectedUserNames = new List<string>
                {
                    _user.UserName(),
                },
                AdministratorName = _user.UserName(),
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

    public async Task<ModelResponse<Room>> ChangeRoomPuzzleAsync(ChangePuzzleRequest request)
    {
        var response = new ModelResponse<Room>();

        var room = await _roomRepository.ReadByNameAsync(request.RoomName);

        if (room is null)
        {
            response.ErrorMessage = "Room not exists";

            return response;
        }

        room.Settings.Puzzle = request.Puzzle;
        room.Solves = new List<Solve>();

        await _roomRepository.ReplaceAsync(room);

        response.Model = room;
        response.IsSuccess = true;

        return response;
    }

    public async Task<RoomOperationResponse<Room>> RemoveRoomAsync(string roomId)
    {
        var response = new RoomOperationResponse<Room>();

        var removed = await _roomRepository.RemoveAsync(roomId);

        if (removed)
        {
            response.IsSuccess = true;
            response.RoomName = roomId;
        }

        return response;
    }

    public async Task<List<RoomDisplayDataResponse>> GetAllRoomsDataAsync()
    {
        var allRooms = await _roomRepository.ReadAllAsync();

        return allRooms
            .Select(
                r => new RoomDisplayDataResponse
                {
                    Id = r.Id,
                    IsOpen = r.Settings.IsOpen,
                    RoomName = r.Name,
                    Puzzle = r.Settings.Puzzle,
                    AdministratorName = r.AdministratorName,
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

        var isAdminEmpty = string.IsNullOrEmpty(room.AdministratorName);
        var isEmptyRoom = !room.ConnectedUserNames.Any();

        if (isAdminEmpty || isEmptyRoom)
        {
            room.AdministratorName = _user.UserName();
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

        var roomUpdate = _roomRepository.ReplaceAsync(room);
        var userUpdate = UpdateUserLastRoomAsync(room.Name);

        await Task.WhenAll(roomUpdate, userUpdate);

        result.IsSuccess = true;
        result.Model = _mapper.Map<RoomResponse>(room);
        AddScrambleImageToLastSolve(result.Model);

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

                var roomUpdate = _roomRepository.InsertUserResultPatch(
                    room.Name,
                    room.Solves.IndexOf(roomSolve),
                    newResult);
                var userUpdate = _userRepository.InsertSolveAsync(
                    new UserSolve
                    {
                        RoomName = room.Name,
                        Puzzle = room.Settings.Puzzle,
                        Penalty = request.Penalty,
                        Scramble = roomSolve.Scramble,
                        Time = request.TimeInMilliseconds,
                        DateAdded = DateTime.Now,
                    },
                    _user.UserName());

                await Task.WhenAll(roomUpdate, userUpdate);

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

    private async Task<SolveResponse> CreateNextSolveInRoomAsync(Room room)
    {
        var nextSolveNumber = room.Solves.Count + 1;

        var scrambleWithImage = GetScrambleFromCacheOrGenerate(room);

        var solve = new Solve
        {
            SolveNumber = nextSolveNumber,
            Scramble = scrambleWithImage.Scramble,
            StartTime = DateTime.UtcNow,
        };

        await _roomRepository.InsertNewSolvePatch(room.Name, solve);

        _logger.LogInformation(
            "Created solve: {Solve} in room: {RoomName}",
            solve.ToJsonString(),
            room.Name);

        var solveResponse = _mapper.Map<SolveResponse>(solve);
        solveResponse.ScrambledPuzzleImage = scrambleWithImage.Image;

        return solveResponse;
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

    private async Task UpdateUserLastRoomAsync(string roomName)
    {
        var user = await _userRepository.ReadByNameAsync(_user.UserName());

        user.LastRoomName = roomName;

        await _userRepository.ReplaceAsync(user);
    }

    private ScrambleWithImage GetScrambleFromCacheOrGenerate(Room room)
    {
        var puzzleType = room.Settings.Puzzle;
        if (puzzleType is RoomPuzzle.TwoByTwoCube or RoomPuzzle.ThreeByThreeCube)
        {
            return _scramblerService.GenerateScrambleWithImage(puzzleType);
        }

        var lastScramble = room.CachedScrambles.LastOrDefault();
        ScrambleWithImage result;

        if (lastScramble is null)
        {
            result = _scramblerService.GenerateScrambleWithImage(puzzleType);

            Task.Run(() => UpdateCachedScrambles(room));
        }
        else
        {
            result = new ScrambleWithImage
            {
                Scramble = lastScramble,
                Image = _scramblerService.GetImageFromScramble(lastScramble, puzzleType),
            };

            room.CachedScrambles.Remove(lastScramble);

            if (room.CachedScrambles.Count < MinCachedScrambles)
            {
                Task.Run(() => UpdateCachedScrambles(room));
            }
            else
            {
                Task.Run(
                    () => _roomRepository.ReplaceScrambleCachePatch(
                        room.Name,
                        room.CachedScrambles));
            }
        }

        return result;
    }

    private async Task UpdateCachedScrambles(Room room)
    {
        var cachedScramblesInitialCount = room.CachedScrambles.Count;
        for (var i = 0; i < MaxCachedScrambles - cachedScramblesInitialCount; i++)
        {
            room.CachedScrambles.Add(_scramblerService.GenerateScramble(room.Settings.Puzzle));
        }

        await _roomRepository.ReplaceScrambleCachePatch(room.Name, room.CachedScrambles);
    }

    private void AddScrambleImageToLastSolve(RoomResponse response)
    {
        var lastSolve = response.Solves.MaxBy(s => s.SolveNumber);

        if (lastSolve is not null)
        {
            lastSolve.ScrambledPuzzleImage =
                _scramblerService.GetImageFromScramble(
                    lastSolve.Scramble,
                    response.Settings.Puzzle);
        }
    }
}
