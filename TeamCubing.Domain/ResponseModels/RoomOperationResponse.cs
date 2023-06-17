namespace TeamCubing.Domain.ResponseModels;

public class RoomOperationResponse<T> : ModelResponse<T>
{
    public string RoomName { get; set; }
}
