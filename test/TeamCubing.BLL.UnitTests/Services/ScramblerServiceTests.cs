using FluentAssertions;
using TeamCubing.BLL.Services;
using TeamCubing.Domain.Models;
using Xunit;

namespace TeamCubing.BLL.Tests.Services;

public class ScramblerServiceTests
{
    [Theory]
    [InlineData(RoomPuzzle.ThreeByThreeCube)]
    [InlineData(RoomPuzzle.TwoByTwoCube)]
    [InlineData(RoomPuzzle.FourByFourCube)]
    [InlineData(RoomPuzzle.FiveByFiveCube)]
    [InlineData(RoomPuzzle.SixBySixCube)]
    [InlineData(RoomPuzzle.SevenBySevenCube)]
    public void GenerateCubeScramble_ShouldGenerateValidScrambleAndImage(RoomPuzzle puzzle)
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
