namespace WcaScrambler.Puzzles
{
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string move) : base("Invalid move: " + move)
        {
        }
    }
}