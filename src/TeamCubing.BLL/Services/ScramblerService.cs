using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;
using WcaScrambler.Puzzles;
using WcaScrambler.Utils;
using Puzzle = TeamCubing.Domain.Models.Puzzle;

namespace TeamCubing.BLL.Services;

public class ScramblerService : IScramblerService
{
    private readonly Dictionary<Puzzle, string[]> _puzzleBldMoves = new()
    {
        {
            Puzzle.ThreeByThreeBld, new[]
            {
                " Rw", " Uw", " Lw", " Rw'", " Uw'", " Lw'", " Dw", " Dw'", " Fw'", " Fw", " Rw2", " Uw2", " Lw2", " Rw2'", " Uw2'", " Lw2'", " Dw2", " Dw2'", " Fw2'", " Fw2"
            }
        },
        {
            Puzzle.FourByFourBld, new[]
            {
                " x", " x'", " y", " y'", " z'", " z"
            }
        },
        {
            Puzzle.FiveByFiveBld, new[]
            {
                " 3Rw", " 3Uw", " 3Lw", " 3Rw'", " 3Uw'", " 3Lw'", " 3Dw", " 3Dw'", " 3Fw'", " 3Fw"
            }
        },
    };

    public string GenerateScramble(Puzzle puzzle)
    {
        if (IsCubePuzzle(puzzle))
        {
            var cubeSize = GetCubeSize(puzzle);

            return GenerateScramble(GetCubePuzzle(cubeSize), puzzle);
        }

        if (puzzle is Puzzle.Megaminx)
        {
            return GenerateScramble(new MegaminxPuzzle());
        }

        return null;
    }

    public ScrambleWithImage GenerateScrambleWithImage(Puzzle puzzle)
    {
        var result = new ScrambleWithImage();
        WcaScrambler.Puzzles.Puzzle.PuzzleState puzzleState;
        WcaScrambler.Puzzles.Puzzle puzzleObject;
        if (IsCubePuzzle(puzzle))
        {
            var cubeSize = GetCubeSize(puzzle);
            puzzleObject = GetCubePuzzle(cubeSize);

            (result.Scramble, puzzleState) = ScramblePuzzle(puzzleObject, puzzle);

            result.Image = MapToPuzzleImage((puzzleState as CubePuzzle.CubeState)?.Image, cubeSize);
        }
        else if (puzzle is Puzzle.Megaminx)
        {
            puzzleObject = new MegaminxPuzzle();
            (result.Scramble, _) = ScramblePuzzle(puzzleObject);
        }

        return result;
    }

    public PuzzleImage GetImageFromScramble(string scramble, Puzzle puzzleType)
    {
        if (IsCubePuzzle(puzzleType))
        {
            var cubeSize = GetCubeSize(puzzleType);
            var imageArray =
                (GetCubePuzzle(cubeSize).GetSolvedState().ApplyAlgorithm(scramble) as
                    CubePuzzle.CubeState)?.Image;

            return MapToPuzzleImage(imageArray, cubeSize);
        }

        return null;
    }

    private (string Scramble, WcaScrambler.Puzzles.Puzzle.PuzzleState State) ScramblePuzzle(WcaScrambler.Puzzles.Puzzle puzzleObject, Puzzle? puzzle = null)
    {
        var scramble = GenerateScramble(puzzleObject, puzzle);

        var scrambledState = puzzleObject?.GetSolvedState()?.ApplyAlgorithm(scramble);

        return (scramble, scrambledState);
    }

    private string GenerateScramble(WcaScrambler.Puzzles.Puzzle puzzleObject, Puzzle? puzzle = null)
    {
        var sourceOfRandomness = new Random();
        var scramble = puzzleObject?.GenerateWcaScramble(sourceOfRandomness);

        if (puzzle is not null && _puzzleBldMoves.TryGetValue(puzzle.Value, out var possibleMoves))
        {
            var length = possibleMoves.Length;
            var wideMovesCount = sourceOfRandomness.Next(0, 3);
            for (var i = 0; i < wideMovesCount; i++)
            {
                var wideMove = possibleMoves[sourceOfRandomness.Next(0, length)];

                while (scramble.EndsWith(wideMove) || scramble.EndsWith(wideMove + "'"))
                {
                    wideMove = possibleMoves[sourceOfRandomness.Next(0, length)];
                }

                scramble += wideMove;
            }
        }

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

    private static bool IsCubePuzzle(Puzzle puzzle)
    {
        return puzzle is Puzzle.TwoByTwoCube
            or Puzzle.ThreeByThreeCube
            or Puzzle.FourByFourCube
            or Puzzle.FiveByFiveCube
            or Puzzle.SixBySixCube
            or Puzzle.SevenBySevenCube
            or Puzzle.ThreeByThreeBld
            or Puzzle.FourByFourBld
            or Puzzle.FiveByFiveBld;
    }

    private static int GetCubeSize(Puzzle puzzle)
    {
        // TODO: Replace this piece of shit in all places with normal logic of handling subsets such as bld
        return (int)puzzle % 11;
    }
}
