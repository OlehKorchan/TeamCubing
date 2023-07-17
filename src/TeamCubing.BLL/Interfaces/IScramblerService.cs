using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;

namespace TeamCubing.BLL.Interfaces;

public interface IScramblerService
{
    string GenerateScramble(RoomPuzzle puzzle);

    ScrambleWithImage GenerateScrambleWithImage(RoomPuzzle puzzle);

    PuzzleImage GetImageFromScramble(string scramble, RoomPuzzle puzzleType);
}
