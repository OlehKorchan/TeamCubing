namespace TeamCubing.DAL.Models;

public class RoomSolveResult
{
    public int Id { get; set; }

    public int RoomSolveId { get; set; }

    public RoomSolve RoomSolve { get; set; }

    public string UserId { get; set; }

    public ApplicationUser User { get; set; }

    public int Time { get; set; }
}
