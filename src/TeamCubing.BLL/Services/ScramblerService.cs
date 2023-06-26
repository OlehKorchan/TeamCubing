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
        var cubeSize = (int)puzzle;
        var cube = GetCubePuzzle(cubeSize);

        var sourceOfRandomness = new Random();

        var state = new CubePuzzle.CubeState(cube);

        var scramble = cube.GenerateWcaScramble(sourceOfRandomness);

        var newState = state.ApplyAlgorithm(scramble);
        var image = MapToPuzzleImage((newState as CubePuzzle.CubeState)?.Image, cubeSize);

        return (scramble, image);
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
            _ => new CubePuzzle(size)
        };
    }
}
