using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.ResponseModels;

public class AllResultsByPuzzleResponse
{
    public Puzzle Puzzle { get; set; }

    public List<BaseSolveResult> Results { get; set; }
}
