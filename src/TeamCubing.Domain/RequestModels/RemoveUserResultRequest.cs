namespace TeamCubing.Domain.RequestModels;

public class RemoveUserResultRequest
{
    public string RoomName { get; set; }

    public int SolveNumber { get; set; }

    public string UserName { get; set; }
}
