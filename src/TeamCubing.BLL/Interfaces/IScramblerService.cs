using TeamCubing.Domain.DTO;

namespace TeamCubing.BLL.Interfaces;

public interface IScramblerService
{
    (string Scramble, PuzzleImage Image) GenerateThreeByThreeScrambleWithImage();
}
