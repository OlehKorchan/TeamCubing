using TeamCubing.Domain.Models;

namespace TeamCubing.BLL.Helpers.Extensions;

public static class SolvesExtension
{
    public static BaseSolveResult CalculateBestAverage(
        this IEnumerable<BaseSolveResult> solveResults,
        int averageOf)
    {
        var result = new BaseSolveResult();
        var baseSolveResults = solveResults.ToList();
        var averageResults = new List<BaseSolveResult>();

        for (var i = 0; i < baseSolveResults.Count; i++)
        {
            averageResults.Add(CalculateCurrentAverage(baseSolveResults.Skip(i).Take(averageOf), averageOf));
        }

        var nonDnfResults = averageResults.Where(r => r.Penalty != Penalty.DNF).ToList();

        if (nonDnfResults.Any())
        {
            result.Time = nonDnfResults.MinBy(s => s.Time).Time;
        }

        result.Penalty = Penalty.DNF;

        return result;
    }

    private static BaseSolveResult CalculateCurrentAverage(IEnumerable<BaseSolveResult> solveResults, int averageOf)
    {
        var baseSolveResults = solveResults.ToList();
        var length = baseSolveResults.Count;
        var dnfCount = baseSolveResults.Count(r => r.Penalty == Penalty.DNF);

        var result = new BaseSolveResult();

        if (length != averageOf || dnfCount > 1)
        {
            result.Penalty = Penalty.DNF;

            return result;
        }

        var totalTime = baseSolveResults.Sum(s => s.Time);
        var worstTime = dnfCount == 0
            ? baseSolveResults.MaxBy(r => r.Time).Time
            : baseSolveResults.First(r => r.Penalty == Penalty.DNF).Time;
        var bestTime = baseSolveResults.Where(r => r.Penalty != Penalty.DNF)
            .MinBy(s => s.Time)
            .Time;

        totalTime -= worstTime + bestTime;

        result.Time = totalTime / (length - 2);

        return result;
    }
}
