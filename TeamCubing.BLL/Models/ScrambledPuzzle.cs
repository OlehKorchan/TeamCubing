using WcaScrambler.Puzzles;

namespace TeamCubing.BLL.Models;

public class ScrambledPuzzle
{
    public string Scramble { get; set; }

    public Puzzle.PuzzleState PuzzleState { get; set; }
}
