namespace TeamCubing.DAL.Models;

public class RoomSolve
{
    public int Id { get; set; }

    public int SolveNumber { get; set; }

    public string Scramble { get; set; }

    public DateTime StartTime { get; set; }

    public int RoomId { get; set; }

    public Room Room { get; set; }

    public List<RoomSolveResult> Results { get; set; }
}
