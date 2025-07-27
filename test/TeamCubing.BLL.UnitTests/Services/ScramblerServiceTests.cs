using FluentAssertions;
using TeamCubing.BLL.Services;
using TeamCubing.Domain.Models;
using Xunit;

namespace TeamCubing.BLL.Tests.Services;

public class ScramblerServiceTests
{
    [Theory]
    [InlineData(Puzzle.ThreeByThreeCube)]
    [InlineData(Puzzle.TwoByTwoCube)]
    [InlineData(Puzzle.FourByFourCube)]
    [InlineData(Puzzle.FiveByFiveCube)]
    [InlineData(Puzzle.SixBySixCube)]
    [InlineData(Puzzle.SevenBySevenCube)]
    public void GenerateCubeScramble_ShouldGenerateValidScrambleAndImage(Puzzle puzzle)
    {
        // Arrange
        var sut = new ScramblerService();

        // Act
        var result = sut.GenerateScrambleWithImage(puzzle);

        // Assert
        result.Scramble.Should().NotBeNullOrEmpty();
        result.Image.Should().NotBeNull();
    }
}
