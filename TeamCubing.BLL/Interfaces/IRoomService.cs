using TeamCubing.BLL.Models;
using TeamCubing.DAL.Models;

namespace TeamCubing.BLL.Interfaces;

public interface IRoomService
{
    Task<RoomCheckAccessResult> CheckAccessAsync(string roomName);

    Task<RoomDto> CreateRoomAsync(RoomLoginDto loginDto);

    Task<List<string>> GetAllAsync();

    Task<string> LeaveCurrentRoomAsync();

    Task<RoomDto> GetRoomWithUsersAndSolvesAsync(int roomId);

    Task<RoomDto> GetRoomByNameWithUsersAsync(string roomName);

    Task<RoomLoginResult> LoginToRoomAsync(RoomLoginDto loginDto);

    Task<RoomSolveResultDto> AddUserResultAsync(RoomSolveResult roomSolveResult);
    public Task<bool> IsSolveFinished(int solveId);
    public Task<RoomSolveDto> CreateNextSolveInRoomAsync(RoomDto room);
}
