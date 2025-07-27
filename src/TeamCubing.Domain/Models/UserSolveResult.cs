namespace TeamCubing.Domain.Models;

public class UserSolveResult : BaseSolveResult
{
    public string RoomName { get; set; }

    public int SolveNumber { get; set; }

    public string Scramble { get; set; }

    public Puzzle Puzzle { get; set; }

    public DateTime DateAdded { get; set; }
}