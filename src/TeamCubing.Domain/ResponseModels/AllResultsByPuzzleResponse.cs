using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.ResponseModels;

public class AllResultsByPuzzleResponse
{
    public RoomPuzzle Puzzle { get; set; }

    public List<BaseSolveResult> Results { get; set; }
}
