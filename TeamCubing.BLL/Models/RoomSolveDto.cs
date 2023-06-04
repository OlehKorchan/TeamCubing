namespace TeamCubing.BLL.Models;

public class RoomSolveDto
{
    public int Id { get; set; }

    public int SolveNumber { get; set; }

    public List<RoomSolveResultDto> Results { get; set; }

    public int RoomId { get; set; }

    public string Scramble { get; set; }
}
