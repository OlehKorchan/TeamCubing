namespace TeamCubing.Domain.Models;

public class UserSolve
{
    public string RoomName { get; set; }

    public int SolveNumber { get; set; }

    public string Scramble { get; set; }

    public RoomPuzzle Puzzle { get; set; }

    public int Time { get; set; }

    public Penalty Penalty { get; set; }

    public DateTime DateAdded { get; set; }
}
