using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;
using TeamCubing.Domain.ResponseModels;

namespace TeamCubing.BLL.Interfaces;

public interface IRoomService
{
    Task<RoomCheckAccessResult> CheckAccessAsync(string roomName);

    Task<ModelResponse<Room>> CreateRoomAsync(RoomCreateRequest loginDto);

    Task<List<RoomDisplayDataResponse>> GetAllRoomsDataAsync();

    Task<string> LeaveLastRoomAsync();

    Task<RoomLoginResponse> LoginToRoomAsync(RoomLoginRequest loginDto);

    Task<RoomOperationResponse<SolveResult>>
        AddUserResultAsync(NewUserResultRequest request);

    Task<SolvePushResponse> PushSolveToRoomAsync(string roomId, bool isForce);

    Task<ModelResponse<Room>> ChangeRoomPuzzleAsync(ChangePuzzleRequest request);

    Task<RoomOperationResponse<Room>> RemoveRoomAsync(string roomId);
}
