using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Services;
using TeamCubing.BLL.Tests.Helpers;
using TeamCubing.DAL.Interfaces;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;
using TeamCubing.Domain.ResponseModels;
using Xunit;

namespace TeamCubing.BLL.Tests.Services;

public class RoomServiceTests
{
    private readonly Mock<ILogger<RoomService>> _loggerMock = new();
    private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
    private readonly Mock<IScramblerService> _scramblerMock = new();
    private readonly RoomService _sut;

    public RoomServiceTests()
    {
        _sut = new RoomService(
            _roomRepositoryMock.Object,
            new ApplicationUser
            {
                UserName = TestFixture.CurrentUserName,
            },
            _loggerMock.Object,
            _scramblerMock.Object);
    }

    [Fact]
    public async Task CheckAccessAsync_UserOnceJoinedRoom_ShouldReturnAuthorizedResult()
    {
        // Arrange
        var testRoom = TestFixture.GetRoomWithFinishedSolve();
        var expected = RoomCheckAccessResult.Authorized;

        _roomRepositoryMock
            .Setup(m => m.ReadByNameAsync(It.Is<string>(s => s == testRoom.Name)))
            .ReturnsAsync(testRoom);

        // Act
        var actual = await _sut.CheckAccessAsync(testRoom.Name);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.ReadByNameAsync(It.Is<string>(s => s == testRoom.Name)),
            Times.Once);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task CheckAccessAsync_UserNeverJoinedRoom_ShouldReturnForbiddenResult()
    {
        // Arrange
        var testRoom = TestFixture.GetRoomWithFinishedSolve();
        testRoom.WasOnceConnectedUserNames = new List<string>();
        testRoom.ConnectedUserNames = new List<string>();

        var expected = RoomCheckAccessResult.Forbidden;

        _roomRepositoryMock
            .Setup(m => m.ReadByNameAsync(It.Is<string>(s => s == testRoom.Name)))
            .ReturnsAsync(testRoom);

        // Act
        var actual = await _sut.CheckAccessAsync(testRoom.Name);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.ReadByNameAsync(It.Is<string>(s => s == testRoom.Name)),
            Times.Once);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task CheckAccessAsync_RoomNotFound_ShouldReturnNotFoundResult()
    {
        // Arrange
        var expected = RoomCheckAccessResult.NotFound;

        _roomRepositoryMock
            .Setup(m => m.ReadByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((Room)null);

        // Act
        var actual = await _sut.CheckAccessAsync(string.Empty);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.ReadByNameAsync(It.IsAny<string>()),
            Times.Once);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task CreateRoomAsync_ValidRoomNameAndPassword_ShouldCreateRoom()
    {
        // Arrange
        var testRoom = TestFixture.GetEmptyRoom();
        var request = TestFixture.GetRoomCreateRequest();

        _roomRepositoryMock
            .Setup(
                m => m.InsertAsync(
                    It.Is<Room>(
                        r => r.Name == request.RoomName && r.Password == request.RoomPassword)))
            .ReturnsAsync(testRoom);

        // Act
        var actual = await _sut.CreateRoomAsync(request);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.InsertAsync(
                It.Is<Room>(
                    r => r.Name == request.RoomName && r.Password == request.RoomPassword)),
            Times.Once);
        actual.Model.Should().BeEquivalentTo(testRoom);
        actual.IsSuccess.Should().Be(true);
    }

    [Theory]
    [InlineData("ValidName", null)]
    [InlineData("", "ValidPassword")]
    [InlineData(null, "ValidPassword")]
    [InlineData(null, null)]
    public async Task CreateRoomAsync_InvalidRoomNameAndPassword_ShouldReturnFalseResult(
        string roomName,
        string password)
    {
        // Arrange
        var request = new RoomCreateRequest
        {
            RoomName = roomName,
            RoomPassword = password,
            Settings = new RoomSettings
            {
                IsOpen = false,
            },
        };

        // Act
        var actual = await _sut.CreateRoomAsync(request);

        // Assert
        _roomRepositoryMock.Verify(m => m.InsertAsync(It.IsAny<Room>()), Times.Never);
        actual.Model.Should().BeNull();
        actual.IsSuccess.Should().Be(false);
    }

    [Fact]
    public async Task PushSolveToRoomAsync_NotForceLastSolveFinished_ShouldCreateNewSolve()
    {
        // Arrange
        var testRoom = TestFixture.GetRoomWithFinishedSolve();
        var expectedSolve = TestFixture.GetBaseRoomSolve();
        expectedSolve.SolveNumber++;

        _roomRepositoryMock
            .Setup(m => m.ReadByIdAsync(testRoom.Id))
            .ReturnsAsync(testRoom);

        _scramblerMock
            .Setup(m => m.GenerateThreeByThreeScrambleWithImage())
            .Returns((expectedSolve.Scramble, expectedSolve.ScrambledPuzzleImage));

        // Act
        var actual = await _sut.PushSolveToRoomAsync(testRoom.Id, false);

        // Assert
        _roomRepositoryMock.Verify(m => m.ReadByIdAsync(testRoom.Id), Times.Once);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScrambleWithImage(), Times.Once);
        actual.IsSuccess.Should().BeTrue();
        actual.RoomName.Should().BeEquivalentTo(testRoom.Name);
        actual.Model.Scramble.Should().BeEquivalentTo(expectedSolve.Scramble);
        actual.Model.SolveNumber.Should().Be(expectedSolve.SolveNumber);
    }

    [Fact]
    public async Task PushSolveToRoomAsync_NotForceLastSolveNotFinished_ShouldReturnFalseResult()
    {
        // Arrange
        var testRoom = TestFixture.GetRoomWithEmptySolve();

        _roomRepositoryMock
            .Setup(m => m.ReadByIdAsync(testRoom.Id))
            .ReturnsAsync(testRoom);
        // Act
        var actual = await _sut.PushSolveToRoomAsync(testRoom.Id, false);

        // Assert
        _roomRepositoryMock.Verify(m => m.ReadByIdAsync(testRoom.Id), Times.Once);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScrambleWithImage(), Times.Never);
        actual.IsSuccess.Should().BeFalse();
        actual.RoomName.Should().BeEquivalentTo(testRoom.Name);
        actual.Model.Should().BeNull();
    }

    [Fact]
    public async Task PushSolveToRoomAsync_NotForceAndRoomNotFound_ShouldReturnFalseResult()
    {
        // Arrange
        _roomRepositoryMock
            .Setup(m => m.ReadByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Room)null);

        // Act
        var actual = await _sut.PushSolveToRoomAsync(It.IsAny<string>(), false);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.ReadByIdAsync(It.IsAny<string>()),
            Times.Once);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScrambleWithImage(), Times.Never);
        actual.IsSuccess.Should().BeFalse();
        actual.RoomName.Should().BeNullOrEmpty();
        actual.Model.Should().BeNull();
    }

    [Fact]
    public async Task PushSolveToRoomAsync_ForceLastSolveNotFinished_ShouldCreateNewSolve()
    {
        // Arrange
        var testRoom = TestFixture.GetRoomWithEmptySolve();
        var expectedSolve = TestFixture.GetBaseRoomSolve();
        expectedSolve.SolveNumber++;

        _roomRepositoryMock
            .Setup(m => m.ReadByIdAsync(testRoom.Id))
            .ReturnsAsync(testRoom);
        _scramblerMock
            .Setup(m => m.GenerateThreeByThreeScrambleWithImage())
            .Returns((expectedSolve.Scramble, expectedSolve.ScrambledPuzzleImage));

        // Act
        var actual = await _sut.PushSolveToRoomAsync(testRoom.Id, true);

        // Assert
        _roomRepositoryMock.Verify(m => m.ReadByIdAsync(testRoom.Id), Times.Once);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScrambleWithImage(), Times.Once);
        actual.IsSuccess.Should().BeTrue();
        actual.RoomName.Should().BeEquivalentTo(testRoom.Name);
        actual.Model.Scramble.Should().BeEquivalentTo(expectedSolve.Scramble);
        actual.Model.SolveNumber.Should().Be(expectedSolve.SolveNumber);
    }

    [Fact]
    public async Task PushSolveToRoomAsync_ForceAndRoomNotFound_ShouldReturnFalseResult()
    {
        // Arrange
        _roomRepositoryMock
            .Setup(m => m.ReadByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Room)null);

        // Act
        var actual = await _sut.PushSolveToRoomAsync(It.IsAny<string>(), true);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.ReadByIdAsync(It.IsAny<string>()),
            Times.Once);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScrambleWithImage(), Times.Never);
        actual.IsSuccess.Should().BeFalse();
        actual.RoomName.Should().BeNullOrEmpty();
        actual.Model.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRooms()
    {
        // Arrange
        var testRooms = new List<Room> { TestFixture.GetEmptyRoom() };
        var expected = testRooms.Select(
            r => new RoomDisplayDataResponse
            {
                RoomName = r.Name,
                IsOpen = r.Settings.IsOpen,
                Puzzle = r.Settings.Puzzle,
                ConnectedUsersCount = r.ConnectedUserNames.Count,
                MaxUsersCount = r.Settings.UsersLimit,
            });

        _roomRepositoryMock
            .Setup(m => m.ReadAllAsync())
            .ReturnsAsync(testRooms);

        // Act
        var actual = await _sut.GetAllRoomsDataAsync();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task LeaveAllRoomsAsync_UserInTheRoom_ShouldResetUserRoomAndReturnRoomName()
    {
        // Arrange
        var roomWithUser = TestFixture.GetRoomWithUsers();

        _roomRepositoryMock
            .Setup(m => m.ReadAllRoomsWithUser(TestFixture.CurrentUserName))
            .ReturnsAsync(new List<Room> { roomWithUser });

        // Act
        var previousRoomName = await _sut.LeaveAllRoomsAsync();

        // Assert
        previousRoomName.Should().BeEquivalentTo(roomWithUser.Name);
        _roomRepositoryMock.Verify(
            m => m.ReadAllRoomsWithUser(TestFixture.CurrentUserName),
            Times.Once);
    }

    [Fact]
    public async Task LeaveCurrentRoomAsync_UserNotInTheRoom_ShouldReturnEmpty()
    {
        // Arrange
        var emptyRoom = TestFixture.GetEmptyRoom();

        _roomRepositoryMock
            .Setup(m => m.ReadAllRoomsWithUser(It.IsAny<string>()))
            .ReturnsAsync(new List<Room>());

        // Act
        var previousRoomName = await _sut.LeaveAllRoomsAsync();

        // Assert
        previousRoomName.Should().BeEmpty();
        _roomRepositoryMock.Verify(m => m.ReadAllRoomsWithUser(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoginToRoomAsync_NewUserAndValidRoomNameAndPassword_ShouldLoginSuccessfully()
    {
        // Arrange
        var roomToLogin = TestFixture.GetEmptyRoom();
        roomToLogin.Id = "1";
        var loginRequest = new RoomLoginRequest
        {
            RoomName = roomToLogin.Name,
            RoomPassword = roomToLogin.Password,
        };

        _roomRepositoryMock
            .Setup(
                m => m.ReadByNameAsync(It.Is<string>(s => s == roomToLogin.Name)))
            .ReturnsAsync(roomToLogin);

        // Act
        var result = await _sut.LoginToRoomAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _roomRepositoryMock.Verify(
            m =>
                m.ReadByNameAsync(
                    It.Is<string>(s => s == roomToLogin.Name)),
            Times.Once);
        _roomRepositoryMock.Verify(
            m => m.ReplaceAsync(
                It.Is<Room>(
                    r => r.Name == roomToLogin.Name &&
                         r.WasOnceConnectedUserNames.Contains(TestFixture.CurrentUserName))),
            Times.Once);
    }

    [Fact]
    public async Task LoginToRoomAsync_OldUserAndValidRoomNameAndPassword_ShouldLoginSuccessfully()
    {
        // Arrange
        var roomToLogin = TestFixture.GetRoomWithUsers();
        var loginRequest = new RoomLoginRequest
        {
            RoomName = roomToLogin.Name,
            RoomPassword = roomToLogin.Password,
        };

        _roomRepositoryMock
            .Setup(
                m => m.ReadByNameAsync(It.Is<string>(s => s == roomToLogin.Name)))
            .ReturnsAsync(roomToLogin);

        // Act
        var result = await _sut.LoginToRoomAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Model.ConnectedUserNames.Should().Contain(TestFixture.CurrentUserName);
        result.Model.Id.Should().Be(roomToLogin.Id);
        _roomRepositoryMock.Verify(
            m =>
                m.ReadByNameAsync(
                    It.Is<string>(s => s == roomToLogin.Name)),
            Times.Once);
        _roomRepositoryMock.Verify(
            m => m.ReplaceAsync(
                It.Is<Room>(
                    r => r.Name == roomToLogin.Name &&
                         r.WasOnceConnectedUserNames.Contains(TestFixture.CurrentUserName))),
            Times.Never);
    }

    [Fact]
    public async Task LoginToRoomAsync_OldUserAndInvalidPassword_ShouldLoginSuccessfully()
    {
        // Arrange
        var roomToLogin = TestFixture.GetRoomWithUsers();
        var loginRequest = new RoomLoginRequest
        {
            RoomName = roomToLogin.Name,
            RoomPassword = string.Empty,
        };

        _roomRepositoryMock
            .Setup(
                m => m.ReadByNameAsync(It.Is<string>(s => s == roomToLogin.Name)))
            .ReturnsAsync(roomToLogin);

        // Act
        var result = await _sut.LoginToRoomAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Model.ConnectedUserNames.Should().Contain(TestFixture.CurrentUserName);
        result.Model.Id.Should().Be(roomToLogin.Id);
        _roomRepositoryMock.Verify(
            m =>
                m.ReadByNameAsync(
                    It.Is<string>(s => s == roomToLogin.Name)),
            Times.Once);
        _roomRepositoryMock.Verify(
            m => m.ReplaceAsync(
                It.Is<Room>(
                    r => r.Name == roomToLogin.Name &&
                         r.WasOnceConnectedUserNames.Contains(TestFixture.CurrentUserName))),
            Times.Never);
    }

    [Fact]
    public async Task LoginToRoomAsync_NewUserAndInvalidPassword_ShouldNotLogin()
    {
        // Arrange
        var roomToLogin = TestFixture.GetEmptyRoom();
        roomToLogin.Id = "1";
        var loginRequest = new RoomLoginRequest
        {
            RoomName = roomToLogin.Name,
            RoomPassword = string.Empty,
        };

        _roomRepositoryMock
            .Setup(
                m => m.ReadByNameAsync(It.Is<string>(s => s == roomToLogin.Name)))
            .ReturnsAsync(roomToLogin);

        // Act
        var result = await _sut.LoginToRoomAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _roomRepositoryMock.Verify(
            m =>
                m.ReadByNameAsync(
                    It.Is<string>(s => s == roomToLogin.Name)),
            Times.Once);
        _roomRepositoryMock.Verify(
            m => m.ReplaceAsync(
                It.Is<Room>(
                    r => r.Name == roomToLogin.Name &&
                         r.WasOnceConnectedUserNames.Contains(TestFixture.CurrentUserName))),
            Times.Never);
    }

    [Fact]
    public async Task AddUserResultAsync_ValidResult_ShouldAddResultToSolve()
    {
        // Arrange
        var testRoom = TestFixture.GetRoomWithEmptySolve();
        var solveId = testRoom.Solves.First().SolveNumber;
        var request = new NewUserResultRequest
        {
            RoomId = testRoom.Id,
            SolveNumber = solveId,
            TimeInMilliseconds = 12345,
        };
        var createdResult = new SolveResult
        {
            UserName = TestFixture.CurrentUserName,
            Time = request.TimeInMilliseconds,
        };

        _roomRepositoryMock
            .Setup(m => m.ReadByIdAsync(It.Is<string>(i => i == testRoom.Id)))
            .ReturnsAsync(testRoom);

        // Act
        var result = await _sut.AddUserResultAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.RoomName.Should().BeEquivalentTo(testRoom.Name);
        result.Model.UserName.Should().BeEquivalentTo(TestFixture.CurrentUserName);
        result.Model.Time.Should().Be(request.TimeInMilliseconds);
        _roomRepositoryMock.Verify(
            m =>
                m.ReadByIdAsync(It.Is<string>(i => i == testRoom.Id)),
            Times.Once);
    }
}
