using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;
using WcaScrambler.Puzzles;
using WcaScrambler.Utils;

namespace TeamCubing.BLL.Services;

public class ScramblerService : IScramblerService
{
    public string GenerateScramble(RoomPuzzle puzzle)
    {
        if (IsCubePuzzle(puzzle))
        {
            var cubeSize = (int)puzzle;

            return GenerateScramble(GetCubePuzzle(cubeSize));
        }

        if (puzzle is RoomPuzzle.Megaminx)
        {
            return GenerateScramble(new MegaminxPuzzle());
        }

        return null;
    }

    public ScrambleWithImage GenerateScrambleWithImage(RoomPuzzle puzzle)
    {
        var result = new ScrambleWithImage();
        Puzzle.PuzzleState puzzleState;
        Puzzle puzzleObject;
        if (IsCubePuzzle(puzzle))
        {
            var cubeSize = (int)puzzle;
            puzzleObject = GetCubePuzzle(cubeSize);

            (result.Scramble, puzzleState) = ScramblePuzzle(puzzleObject);

            result.Image = MapToPuzzleImage((puzzleState as CubePuzzle.CubeState)?.Image, cubeSize);
        }
        else if (puzzle is RoomPuzzle.Megaminx)
        {
            puzzleObject = new MegaminxPuzzle();
            (result.Scramble, _) = ScramblePuzzle(puzzleObject);
        }

        return result;
    }

    public PuzzleImage GetImageFromScramble(string scramble, RoomPuzzle puzzleType)
    {
        if (IsCubePuzzle(puzzleType))
        {
            var imageArray =
                (GetCubePuzzle((int)puzzleType).GetSolvedState().ApplyAlgorithm(scramble) as
                    CubePuzzle.CubeState)?.Image;

            return MapToPuzzleImage(imageArray, (int)puzzleType);
        }

        return null;
    }

    private static (string Scramble, Puzzle.PuzzleState State) ScramblePuzzle(Puzzle puzzleObject)
    {
        var scramble = GenerateScramble(puzzleObject);

        var scrambledState = puzzleObject?.GetSolvedState()?.ApplyAlgorithm(scramble);

        return (scramble, scrambledState);
    }

    private static string GenerateScramble(Puzzle puzzleObject)
    {
        var sourceOfRandomness = new Random();
        var scramble = puzzleObject?.GenerateWcaScramble(sourceOfRandomness);

        return scramble;
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
