namespace TeamCubing.Domain.Models;

public class BaseSolveResult
{
    public int TimeMilliseconds { get; set; }

    public Penalty Penalty { get; set; }
}