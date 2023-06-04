namespace TeamCubing.BLL.Models;

public class RoomLoginResult
{
    public bool Result { get; set; }

    public int RoomId { get; set; }

    public List<string> ConnectedUserNames { get; set; }

    public List<RoomSolveDto> Solves { get; set; }
}
