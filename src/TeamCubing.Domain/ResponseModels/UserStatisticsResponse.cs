using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.ResponseModels;

public class UserStatisticsResponse
{
    public List<BestUserResultsByPuzzleResponse> BestResultsByPuzzle { get; set; }

    public List<UserSolveResult> AllResultsByPuzzles { get; set; }
}
