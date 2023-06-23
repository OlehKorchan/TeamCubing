using FluentAssertions;
using TeamCubing.BLL.Services;
using Xunit;

namespace TeamCubing.BLL.Tests.Services;

public class ScramblerServiceTests
{
    [Fact]
    public void GenerateThreeByThreeScramble_ShouldGenerateValidScrambleAndImage()
    {
        // Arrange
        var sut = new ScramblerService();

        // Act
        var result = sut.GenerateThreeByThreeScrambleWithImage();

        // Assert
        result.Scramble.Should().NotBeNullOrEmpty();
        result.Image.Should().NotBeNull();
    }
}
