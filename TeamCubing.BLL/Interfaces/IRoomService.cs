using TeamCubing.BLL.Models;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;
using TeamCubing.Domain.ResponseModels;

namespace TeamCubing.BLL.Interfaces;

public interface IRoomService
{
    Task<RoomCheckAccessResult> CheckAccessAsync(string roomName);

    Task<ModelResponse<Room>> CreateRoomAsync(RoomLoginRequest loginDto);

    Task<List<string>> GetAllRoomNamesAsync();

    Task<List<string>> LeaveAllRoomsAsync();

    Task<RoomLoginResponse> LoginToRoomAsync(RoomLoginRequest loginDto);

    Task<RoomOperationResponse<SolveResult>>
        AddUserResultAsync(NewUserResultRequest request);

    Task<SolvePushResponse> PushSolveToRoomAsync(string roomId, bool isForce);
}
