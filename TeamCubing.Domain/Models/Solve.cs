namespace TeamCubing.Domain.Models;

public class Solve
{
    public int SolveNumber { get; set; }

    public string Scramble { get; set; }

    public DateTime StartTime { get; set; }

    public List<SolveResult> Results { get; set; } = new();
}
