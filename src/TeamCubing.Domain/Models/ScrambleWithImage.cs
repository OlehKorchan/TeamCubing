using TeamCubing.Domain.DTO;

namespace TeamCubing.Domain.Models;

public class ScrambleWithImage
{
    public string Scramble { get; set; }

    public PuzzleImage Image { get; set; }
}
