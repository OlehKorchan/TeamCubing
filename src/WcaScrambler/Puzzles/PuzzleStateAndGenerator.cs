using static WcaScrambler.Puzzles.Puzzle;

namespace WcaScrambler.Puzzles
{
    public class PuzzleStateAndGenerator
    {
        public PuzzleStateAndGenerator(PuzzleState state, string generator)
        {
            State = state;
            Generator = generator;
        }

        public PuzzleState State { get; }
        public string Generator { get; }
    }
}