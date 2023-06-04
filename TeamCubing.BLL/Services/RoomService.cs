using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Models;
using TeamCubing.DAL.Interfaces;
using TeamCubing.DAL.Models;

namespace TeamCubing.BLL.Services;

public class RoomService : IRoomService
{
    private const int RoomSolveMaxDurationSeconds = 120;
    private readonly ILogger<RoomService> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationUser _user;
    private readonly IUserService _userService;

    public RoomService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ApplicationUser user,
        IUserService userService,
        ILogger<RoomService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _user = user;
        _userService = userService;
        _logger = logger;
    }

    public async Task<RoomCheckAccessResult> CheckAccessAsync(string roomName)
    {
        var room = await GetRoomByNameWithUsersAsync(roomName);

        if (room == null)
        {
            return RoomCheckAccessResult.NotFound;
        }

        return room.ConnectedUserNames.Contains(_user.UserName) ||
               room.WasOnceConnectedUsers.Contains(_user.UserName)
            ? RoomCheckAccessResult.Authorized
            : RoomCheckAccessResult.Forbidden;
    }

    public async Task<RoomDto> CreateRoomAsync(RoomLoginDto loginDto)
    {
        if (string.IsNullOrEmpty(loginDto.RoomName))
        {
            throw new ArgumentNullException();
        }

        var createdRoom = await RunSqlOperationSafely(
            async () =>
            {
                var room = await _unitOfWork
                    .GetRepository<Room>()
                    .CreateOneAsync(
                        new Room
                        {
                            Name = loginDto.RoomName,
                            Password = loginDto.RoomPassword,
                            WasOnceConnectedUsers = string.Empty,
                        });

                await _unitOfWork.SaveAsync();

                return room;
            });

        if (createdRoom is null)
        {
            return null;
        }

        await CreateNextSolveInRoomAsync(_mapper.Map<Room, RoomDto>(createdRoom));

        return _mapper.Map<Room, RoomDto>(createdRoom);
    }

    public async Task<RoomSolveDto> CreateNextSolveInRoomAsync(RoomDto room)
    {
        var nextSolveNumber = room.Solves.Count + 1;

        var created = await RunSqlOperationSafely(
            async () =>
            {
                var solve = await _unitOfWork.GetRepository<RoomSolve>()
                    .CreateOneAsync(
                        new RoomSolve
                        {
                            RoomId = room.Id,
                            SolveNumber = nextSolveNumber,
                            Scramble = "GENERATING...",
                            StartTime = DateTime.UtcNow,
                        });

                await _unitOfWork.SaveAsync();

                return solve;
            });

        if (created is null)
        {
            return null;
        }

        return new RoomSolveDto
        {
            Id = created.Id,
            SolveNumber = created.SolveNumber,
            Scramble = created.Scramble,
            Results = new List<RoomSolveResultDto>(),
            RoomId = room.Id,
        };
    }

    public async Task<List<string>> GetAllAsync()
    {
        return (await _unitOfWork.GetRepository<Room>().GetManyAsync(r => true))
            .Select(r => r.Name)
            .ToList();
    }

    public async Task<string> LeaveCurrentRoomAsync()
    {
        var previousRoomName = await UpdateCurrentUserRoomAsync(null);

        await _unitOfWork.SaveAsync();

        return previousRoomName;
    }

    public async Task<RoomDto> GetRoomWithUsersAndSolvesAsync(int roomId)
    {
        return _mapper.Map<Room, RoomDto>(
            await _unitOfWork.GetRepository<Room>()
                .AsQueryable()
                .Include(r => r.Users)
                .Include(r => r.Solves)
                .FirstOrDefaultAsync(r => r.Id == roomId));
    }

    public async Task<RoomDto> GetRoomByNameWithUsersAsync(string roomName)
    {
        var room = await _unitOfWork.GetRepository<Room>()
            .AsQueryable()
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Name == roomName);

        return _mapper.Map<Room, RoomDto>(room);
    }

    public async Task<RoomLoginResult> LoginToRoomAsync(RoomLoginDto loginDto)
    {
        var (validationResult, room) = await PerformRoomValidationAsync(loginDto);

        if (!validationResult || IsLoginAttemptFailed(room, loginDto, out var userNotInRoom))
        {
            return new RoomLoginResult
            {
                Result = false,
            };
        }

        var successResult = new RoomLoginResult
        {
            Result = true,
            RoomId = room.Id,
            ConnectedUserNames = room.Users.Select(u => u.UserName).ToList(),
            Solves = _mapper.Map<List<RoomSolve>, List<RoomSolveDto>>(room.Solves),
        };

        if (userNotInRoom)
        {
            successResult.ConnectedUserNames.Add(_user.UserName);

            room.WasOnceConnectedUsers += _user.UserName + ',';

            _unitOfWork.GetRepository<Room>().UpdateOne(room);
        }

        await UpdateCurrentUserRoomAsync(room.Id);

        await _unitOfWork.SaveAsync();

        return successResult;
    }

    public async Task<RoomSolveResultDto> AddUserResultAsync(RoomSolveResult roomSolveResult)
    {
        var created = await _unitOfWork.GetRepository<RoomSolveResult>()
            .CreateOneAsync(roomSolveResult);
        await _unitOfWork.SaveAsync();

        return new RoomSolveResultDto
        {
            Id = created.Id,
            UserName = _user.UserName,
            RoomSolveId = created.RoomSolveId,
            Time = created.Time,
        };
    }

    public async Task<bool> IsSolveFinished(int solveId)
    {
        var solve = await _unitOfWork.GetRepository<RoomSolve>()
            .AsQueryable()
            .Include(s => s.Room)
            .Include(s => s.Results)
            .FirstOrDefaultAsync(s => s.Id == solveId);

        return solve.Room.Users.All(un => solve.Results.Any(r => r.User.UserName == un.UserName)) ||
               solve.StartTime.AddSeconds(RoomSolveMaxDurationSeconds) <= DateTime.UtcNow;
    }

    private async Task<Room> GetRoomWithEverythingAsync(string roomName)
    {
        return await _unitOfWork
            .GetRepository<Room>()
            .AsQueryable()
            .Include(r => r.Users)
            .Include(r => r.Solves)
            .ThenInclude(s => s.Results)
            .ThenInclude(r => r.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                r => r.Name == roomName);
    }

    private async Task<ApplicationUser> GetCurrentUserTrackingAsync()
    {
        return await _unitOfWork
            .GetRepository<ApplicationUser>()
            .GetOneTrackingAsync(u => u.UserName == _user.UserName);
    }

    private async Task<(bool, Room)> PerformRoomValidationAsync(RoomLoginDto loginDto)
    {
        if (string.IsNullOrEmpty(loginDto.RoomName))
        {
            return (false, null);
        }

        var room = await GetRoomWithEverythingAsync(loginDto.RoomName);

        return room == null ? (false, null) : (true, room);
    }

    private bool IsLoginAttemptFailed(Room room, RoomLoginDto loginDto, out bool isUserNotInRoom)
    {
        var roomUser = room.WasOnceConnectedUsers
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(un => un == _user.UserName);

        isUserNotInRoom = string.IsNullOrEmpty(roomUser);

        if (isUserNotInRoom && room.Password != loginDto.RoomPassword)
        {
            return true;
        }

        return false;
    }

    private async Task<string> UpdateCurrentUserRoomAsync(int? newRoomId)
    {
        var currentUser = await GetCurrentUserTrackingAsync();

        var oldRoomName = string.Empty;

        if (currentUser.RoomId.HasValue)
        {
            var oldRoom = await GetRoomWithUsersAndSolvesAsync(currentUser.RoomId.Value);

            if (oldRoom is not null)
            {
                oldRoomName = oldRoom.Name;
            }
        }

        currentUser.RoomId = newRoomId;

        _unitOfWork.GetRepository<ApplicationUser>().UpdateOne(currentUser);

        return oldRoomName;
    }

    private async Task<T> RunSqlOperationSafely<T>(Func<Task<T>> sqlOperation)
    {
        try
        {
            return await sqlOperation.Invoke();
        }
        catch (Exception e)
        {
            _logger.LogError("Sql error occured {ErrorMessage}", e.Message);

            //TODO: Handle this in other way, i.e. add envelope with status
            return default;
        }
    }
}
