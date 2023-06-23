using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.DTO;
using WcaScrambler.Puzzles;

namespace TeamCubing.BLL.Services;

public class ScramblerService : IScramblerService
{
    public (string Scramble, PuzzleImage Image) GenerateThreeByThreeScrambleWithImage()
    {
        var cube = new ThreeByThreeCubePuzzle();

        var sourceOfRandomness = new Random();

        var state = new CubePuzzle.CubeState(cube);

        var scramble = cube.GenerateWcaScramble(sourceOfRandomness);

        var newState = state.ApplyAlgorithm(scramble);
        var image = MapToPuzzleImage((newState as CubePuzzle.CubeState)?.Image);

        return (scramble, image);
    }

    private static PuzzleImage MapToPuzzleImage(int[][][] image)
    {
        var puzzleImage = new PuzzleImage
        {
            Faces = new CubeFace[6],
        };

        for (var i = 0; i < image.Length; i++)
        {
            var face = new CubeFace
            {
                Colors = new[]
                {
                    new Color[3],
                    new Color[3],
                    new Color[3],
                },
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
}
