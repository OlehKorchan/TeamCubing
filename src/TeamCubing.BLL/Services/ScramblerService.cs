using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;
using WcaScrambler.Puzzles;
using WcaScrambler.Utils;

namespace TeamCubing.BLL.Services;

public class ScramblerService : IScramblerService
{
    public (string Scramble, PuzzleImage Image) GenerateScrambleWithImage(RoomPuzzle puzzle)
    {
        Puzzle.PuzzleState puzzleState;
        Puzzle puzzleObject;
        string scramble = null;
        PuzzleImage image = null;
        if (IsCubePuzzle(puzzle))
        {
            var cubeSize = (int)puzzle;
            puzzleObject = GetCubePuzzle(cubeSize);

            (scramble, puzzleState) = ScramblePuzzle(puzzleObject);

            image = MapToPuzzleImage((puzzleState as CubePuzzle.CubeState)?.Image, cubeSize);
        }
        else if (puzzle is RoomPuzzle.Megaminx)
        {
            puzzleObject = new MegaminxPuzzle();
            (scramble, _) = ScramblePuzzle(puzzleObject);
        }

        return (scramble, image);
    }

    private static (string Scramble, Puzzle.PuzzleState State) ScramblePuzzle(Puzzle puzzleObject)
    {
        var sourceOfRandomness = new Random();
        var scramble = puzzleObject?.GenerateWcaScramble(sourceOfRandomness);

        var scrambledState = puzzleObject?.GetSolvedState()?.ApplyAlgorithm(scramble);

        return (scramble, scrambledState);
    }

    private static PuzzleImage MapToPuzzleImage(IReadOnlyList<int[][]> image, int puzzleSize)
    {
        var puzzleImage = new PuzzleImage
        {
            Faces = new CubeFace[6],
        };

        for (var i = 0; i < image.Count; i++)
        {
            var face = new CubeFace
            {
                Colors = ArrayExtension.New<Color>(puzzleSize, puzzleSize),
            };
            for (var j = 0; j < image[i].Length; j++)
            {
                for (var k = 0; k < image[i][j].Length; k++)
                {
                    face.Colors[j][k] = (Color)image[i][j][k];
                }
            }

            puzzleImage.Faces[i] = face;
        }

        return puzzleImage;
    }

    private static CubePuzzle GetCubePuzzle(int size)
    {
        return size switch
        {
            < 2 or > 7 => throw new ArgumentException(
                "Cube size should be between 2 and 7",
                nameof(size)),
            3 => new ThreeByThreeCubePuzzle(),
            _ => new CubePuzzle(size),
        };
    }

    private static bool IsCubePuzzle(RoomPuzzle puzzle)
    {
        return puzzle is RoomPuzzle.TwoByTwoCube
            or RoomPuzzle.ThreeByThreeCube
            or RoomPuzzle.FourByFourCube
            or RoomPuzzle.FiveByFiveCube
            or RoomPuzzle.SixBySixCube
            or RoomPuzzle.SevenBySevenCube;
    }
}
