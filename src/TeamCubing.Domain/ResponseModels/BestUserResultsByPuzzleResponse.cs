using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.ResponseModels;

public class BestUserResultsByPuzzleResponse
{
    public RoomPuzzle Event { get; set; }

    public BaseSolveResult Single { get; set; }

    public BaseSolveResult Average { get; set; }
}
