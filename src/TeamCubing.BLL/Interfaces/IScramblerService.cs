using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;

namespace TeamCubing.BLL.Interfaces;

public interface IScramblerService
{
    string GenerateScramble(Puzzle puzzle);

    ScrambleWithImage GenerateScrambleWithImage(Puzzle puzzle);

    PuzzleImage GetImageFromScramble(string scramble, Puzzle puzzleType);
}
