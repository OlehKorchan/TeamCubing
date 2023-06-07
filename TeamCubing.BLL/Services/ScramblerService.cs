using TeamCubing.BLL.Interfaces;
using WcaScrambler.Puzzles;

namespace TeamCubing.BLL.Services;

public class ScramblerService : IScramblerService
{
    public string GenerateThreeByThreeScramble()
    {
        var cube = new ThreeByThreeCubePuzzle();

        var sourceOfRandomness = new Random();

        var scramble = cube.GenerateWcaScramble(sourceOfRandomness);

        return scramble;
    }
}
