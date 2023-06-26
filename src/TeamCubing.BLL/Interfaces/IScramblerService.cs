using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;

namespace TeamCubing.BLL.Interfaces;

public interface IScramblerService
{
    (string Scramble, PuzzleImage Image) GenerateScrambleWithImage(RoomPuzzle puzzle);
}
