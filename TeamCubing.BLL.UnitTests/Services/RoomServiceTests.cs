using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Services;
using TeamCubing.BLL.Tests.Helpers;
using TeamCubing.DAL.Interfaces;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.RequestModels;
using Xunit;

namespace TeamCubing.BLL.Tests.Services;

public class RoomServiceTests
{
    private readonly Mock<ILogger<RoomService>> _loggerMock = new();
    private readonly IMapper _mapper;
    private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
    private readonly Mock<IRepository<RoomSolve>> _roomSolveRepositoryMock = new();
    private readonly Mock<IScramblerService> _scramblerMock = new();
    private readonly RoomService _sut;
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();

    public RoomServiceTests()
    {
        _uowMock
            .Setup(m => m.RoomRepository)
            .Returns(_roomRepositoryMock.Object);
        _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<GeneralProfile>()));

        _sut = new RoomService(
            _uowMock.Object,
            _mapper,
            TestFixture.GetCurrentUser(),
            _userServiceMock.Object,
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
            .Setup(m => m.GetRoomByNameWithUsersAsync(It.Is<string>(s => s == testRoom.Name)))
            .ReturnsAsync(testRoom);

        // Act
        var actual = await _sut.CheckAccessAsync(testRoom.Name);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.GetRoomByNameWithUsersAsync(It.Is<string>(s => s == testRoom.Name)),
            Times.Once);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task CheckAccessAsync_UserNeverJoinedRoom_ShouldReturnForbiddenResult()
    {
        // Arrange
        var testRoom = TestFixture.GetRoomWithFinishedSolve();
        testRoom.WasOnceConnectedUsers = string.Empty;
        testRoom.Users = new List<ApplicationUser>();

        var expected = RoomCheckAccessResult.Forbidden;

        _roomRepositoryMock
            .Setup(m => m.GetRoomByNameWithUsersAsync(It.Is<string>(s => s == testRoom.Name)))
            .ReturnsAsync(testRoom);

        // Act
        var actual = await _sut.CheckAccessAsync(testRoom.Name);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.GetRoomByNameWithUsersAsync(It.Is<string>(s => s == testRoom.Name)),
            Times.Once);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task CheckAccessAsync_RoomNotFound_ShouldReturnNotFoundResult()
    {
        // Arrange
        var expected = RoomCheckAccessResult.NotFound;

        _roomRepositoryMock
            .Setup(m => m.GetRoomByNameWithUsersAsync(It.IsAny<string>()))
            .ReturnsAsync((SqlRoom)null);

        // Act
        var actual = await _sut.CheckAccessAsync(string.Empty);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.GetRoomByNameWithUsersAsync(It.IsAny<string>()),
            Times.Once);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task CreateRoomAsync_ValidRoomNameAndPassword_ShouldCreateRoom()
    {
        // Arrange
        var testRoom = TestFixture.GetEmptyRoom();
        var request = TestFixture.GetRoomLoginRequest();
        var expectedRoom = _mapper.Map<RoomDto>(testRoom);

        _roomRepositoryMock
            .Setup(
                m => m.CreateOneAsync(
                    It.Is<SqlRoom>(
                        r => r.Name == request.RoomName && r.Password == request.RoomPassword)))
            .ReturnsAsync(testRoom);

        // Act
        var actual = await _sut.CreateRoomAsync(request);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.CreateOneAsync(
                It.Is<SqlRoom>(
                    r => r.Name == request.RoomName && r.Password == request.RoomPassword)),
            Times.Once);
        _uowMock.Verify(m => m.SaveAsync(), Times.Once);
        actual.Model.Should().BeEquivalentTo(expectedRoom);
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
        var request = new RoomLoginDto
        {
            RoomName = roomName,
            RoomPassword = password,
        };

        // Act
        var actual = await _sut.CreateRoomAsync(request);

        // Assert
        _roomRepositoryMock.Verify(m => m.CreateOneAsync(It.IsAny<SqlRoom>()), Times.Never);
        _uowMock.Verify(m => m.SaveAsync(), Times.Never);
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
            .Setup(m => m.GetRoomByIdWithEverythingAsync(testRoom.Id))
            .ReturnsAsync(testRoom);
        _uowMock
            .Setup(m => m.GetRepository<RoomSolve>())
            .Returns(_roomSolveRepositoryMock.Object);
        _roomSolveRepositoryMock
            .Setup(
                m => m.CreateOneAsync(
                    It.Is<RoomSolve>(
                        r => r.SolveNumber == expectedSolve.SolveNumber &&
                             r.Scramble == expectedSolve.Scramble &&
                             r.RoomId == expectedSolve.RoomId)))
            .ReturnsAsync(expectedSolve);
        _scramblerMock
            .Setup(m => m.GenerateThreeByThreeScramble())
            .Returns(expectedSolve.Scramble);

        // Act
        var actual = await _sut.PushSolveToRoomAsync(testRoom.Id, false);

        // Assert
        _roomRepositoryMock.Verify(m => m.GetRoomByIdWithEverythingAsync(testRoom.Id), Times.Once);
        _roomSolveRepositoryMock.Verify(
            m => m.CreateOneAsync(
                It.Is<RoomSolve>(
                    r => r.SolveNumber == expectedSolve.SolveNumber &&
                         r.Scramble == expectedSolve.Scramble &&
                         r.RoomId == expectedSolve.RoomId)),
            Times.Once);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScramble(), Times.Once);
        _uowMock.Verify(m => m.SaveAsync(), Times.Once);
        actual.IsSuccess.Should().BeTrue();
        actual.RoomName.Should().BeEquivalentTo(testRoom.Name);
        actual.Model.RoomId.Should().Be(testRoom.Id);
        actual.Model.Scramble.Should().BeEquivalentTo(expectedSolve.Scramble);
        actual.Model.SolveNumber.Should().Be(expectedSolve.SolveNumber);
    }

    [Fact]
    public async Task PushSolveToRoomAsync_NotForceLastSolveNotFinished_ShouldReturnFalseResult()
    {
        // Arrange
        var testRoom = TestFixture.GetRoomWithEmptySolve();

        _roomRepositoryMock
            .Setup(m => m.GetRoomByIdWithEverythingAsync(testRoom.Id))
            .ReturnsAsync(testRoom);
        _uowMock
            .Setup(m => m.GetRepository<RoomSolve>())
            .Returns(_roomSolveRepositoryMock.Object);

        // Act
        var actual = await _sut.PushSolveToRoomAsync(testRoom.Id, false);

        // Assert
        _roomRepositoryMock.Verify(m => m.GetRoomByIdWithEverythingAsync(testRoom.Id), Times.Once);
        _roomSolveRepositoryMock.Verify(
            m => m.CreateOneAsync(
                It.IsAny<RoomSolve>()),
            Times.Never);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScramble(), Times.Never);
        _uowMock.Verify(m => m.SaveAsync(), Times.Never);
        actual.IsSuccess.Should().BeFalse();
        actual.RoomName.Should().BeEquivalentTo(testRoom.Name);
        actual.Model.Should().BeNull();
    }

    [Fact]
    public async Task PushSolveToRoomAsync_NotForceAndRoomNotFound_ShouldReturnFalseResult()
    {
        // Arrange
        _roomRepositoryMock
            .Setup(m => m.GetRoomByIdWithEverythingAsync(It.IsAny<int>()))
            .ReturnsAsync((SqlRoom)null);

        // Act
        var actual = await _sut.PushSolveToRoomAsync(It.IsAny<int>(), false);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.GetRoomByIdWithEverythingAsync(It.IsAny<int>()),
            Times.Once);
        _roomSolveRepositoryMock.Verify(
            m => m.CreateOneAsync(
                It.IsAny<RoomSolve>()),
            Times.Never);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScramble(), Times.Never);
        _uowMock.Verify(m => m.SaveAsync(), Times.Never);
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
            .Setup(m => m.GetRoomByIdWithEverythingAsync(testRoom.Id))
            .ReturnsAsync(testRoom);
        _uowMock
            .Setup(m => m.GetRepository<RoomSolve>())
            .Returns(_roomSolveRepositoryMock.Object);
        _roomSolveRepositoryMock
            .Setup(
                m => m.CreateOneAsync(
                    It.Is<RoomSolve>(
                        r => r.SolveNumber == expectedSolve.SolveNumber &&
                             r.Scramble == expectedSolve.Scramble &&
                             r.RoomId == expectedSolve.RoomId)))
            .ReturnsAsync(expectedSolve);
        _scramblerMock
            .Setup(m => m.GenerateThreeByThreeScramble())
            .Returns(expectedSolve.Scramble);

        // Act
        var actual = await _sut.PushSolveToRoomAsync(testRoom.Id, true);

        // Assert
        _roomRepositoryMock.Verify(m => m.GetRoomByIdWithEverythingAsync(testRoom.Id), Times.Once);
        _roomSolveRepositoryMock.Verify(
            m => m.CreateOneAsync(
                It.Is<RoomSolve>(
                    r => r.SolveNumber == expectedSolve.SolveNumber &&
                         r.Scramble == expectedSolve.Scramble &&
                         r.RoomId == expectedSolve.RoomId)),
            Times.Once);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScramble(), Times.Once);
        _uowMock.Verify(m => m.SaveAsync(), Times.Once);
        actual.IsSuccess.Should().BeTrue();
        actual.RoomName.Should().BeEquivalentTo(testRoom.Name);
        actual.Model.RoomId.Should().Be(testRoom.Id);
        actual.Model.Scramble.Should().BeEquivalentTo(expectedSolve.Scramble);
        actual.Model.SolveNumber.Should().Be(expectedSolve.SolveNumber);
    }

    [Fact]
    public async Task PushSolveToRoomAsync_ForceAndRoomNotFound_ShouldReturnFalseResult()
    {
        // Arrange
        _roomRepositoryMock
            .Setup(m => m.GetRoomByIdWithEverythingAsync(It.IsAny<int>()))
            .ReturnsAsync((SqlRoom)null);

        // Act
        var actual = await _sut.PushSolveToRoomAsync(It.IsAny<int>(), true);

        // Assert
        _roomRepositoryMock.Verify(
            m => m.GetRoomByIdWithEverythingAsync(It.IsAny<int>()),
            Times.Once);
        _roomSolveRepositoryMock.Verify(
            m => m.CreateOneAsync(
                It.IsAny<RoomSolve>()),
            Times.Never);
        _scramblerMock.Verify(m => m.GenerateThreeByThreeScramble(), Times.Never);
        _uowMock.Verify(m => m.SaveAsync(), Times.Never);
        actual.IsSuccess.Should().BeFalse();
        actual.RoomName.Should().BeNullOrEmpty();
        actual.Model.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRoomNames()
    {
        // Arrange
        var testRooms = new List<SqlRoom> { TestFixture.GetEmptyRoom() };
        var expected = testRooms.Select(r => r.Name);

        _roomRepositoryMock
            .Setup(m => m.GetManyAsync(It.IsAny<Expression<Func<SqlRoom, bool>>>()))
            .ReturnsAsync(testRooms);

        // Act
        var actual = await _sut.GetAllRoomNamesAsync();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task LeaveCurrentRoomAsync_UserInTheRoom_ShouldResetUserRoomAndReturnRoomName()
    {
        // Arrange
        var currentUser = TestFixture.GetCurrentUser();
        var userRepoMock = new Mock<IRepository<ApplicationUser>>();
        var roomWithUser = TestFixture.GetRoomWithUsers();
        currentUser.RoomId = roomWithUser.Id;

        _uowMock
            .Setup(m => m.GetRepository<ApplicationUser>())
            .Returns(userRepoMock.Object);
        userRepoMock
            .Setup(m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(currentUser);
        _roomRepositoryMock
            .Setup(m => m.GetRoomByIdWithUsersAsync(roomWithUser.Id))
            .ReturnsAsync(roomWithUser);

        // Act
        var previousRoomName = await _sut.LeaveAllRoomsAsync(TODO);

        // Assert
        previousRoomName.Should().BeEquivalentTo(roomWithUser.Name);
        userRepoMock.Verify(
            m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()),
            Times.Once);
        userRepoMock.Verify(
            m => m.UpdateOne(
                It.Is<ApplicationUser>(
                    u => u.UserName == currentUser.UserName &&
                         u.Id == currentUser.Id &&
                         u.RoomId == null)),
            Times.Once);
        _roomRepositoryMock.Verify(m => m.GetRoomByIdWithUsersAsync(roomWithUser.Id), Times.Once);
        _uowMock.Verify(m => m.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task LeaveCurrentRoomAsync_UserNotInTheRoom_ShouldReturnEmpty()
    {
        // Arrange
        var currentUser = TestFixture.GetCurrentUser();
        var userRepoMock = new Mock<IRepository<ApplicationUser>>();

        _uowMock
            .Setup(m => m.GetRepository<ApplicationUser>())
            .Returns(userRepoMock.Object);
        userRepoMock
            .Setup(m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(currentUser);

        // Act
        var previousRoomName = await _sut.LeaveAllRoomsAsync(TODO);

        // Assert
        previousRoomName.Should().BeEmpty();
        userRepoMock.Verify(
            m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()),
            Times.Once);
        userRepoMock.Verify(
            m => m.UpdateOne(
                It.Is<ApplicationUser>(
                    u => u.UserName == currentUser.UserName &&
                         u.Id == currentUser.Id &&
                         u.RoomId == null)),
            Times.Once);
        _roomRepositoryMock.Verify(m => m.GetRoomByIdWithUsersAsync(It.IsAny<int>()), Times.Never);
        _uowMock.Verify(m => m.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task LoginToRoomAsync_NewUserAndValidRoomNameAndPassword_ShouldLoginSuccessfully()
    {
        // Arrange
        var userRepoMock = new Mock<IRepository<ApplicationUser>>();
        var roomToLogin = TestFixture.GetEmptyRoom();
        roomToLogin.Id = 1;
        var loginRequest = new RoomLoginDto
        {
            RoomName = roomToLogin.Name,
            RoomPassword = roomToLogin.Password,
        };
        var currentUser = TestFixture.GetCurrentUser();

        _uowMock
            .Setup(m => m.GetRepository<ApplicationUser>())
            .Returns(userRepoMock.Object);
        userRepoMock
            .Setup(m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(currentUser);
        _roomRepositoryMock
            .Setup(
                m => m.GetRoomByNameWithEverythingAsync(It.Is<string>(s => s == roomToLogin.Name)))
            .ReturnsAsync(roomToLogin);

        // Act
        var result = await _sut.LoginToRoomAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ConnectedUserNames.Should().Contain(currentUser.UserName);
        result.RoomId.Should().Be(roomToLogin.Id);
        _roomRepositoryMock.Verify(
            m =>
                m.GetRoomByNameWithEverythingAsync(
                    It.Is<string>(s => s == roomToLogin.Name)),
            Times.Once);
        _roomRepositoryMock.Verify(
            m => m.UpdateOne(
                It.Is<SqlRoom>(
                    r => r.Name == roomToLogin.Name &&
                         r.WasOnceConnectedUsers.Contains(currentUser.UserName))),
            Times.Once);
        userRepoMock.Verify(
            m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()),
            Times.Once);
        userRepoMock.Verify(
            m => m.UpdateOne(
                It.Is<ApplicationUser>(
                    u => u.UserName == currentUser.UserName &&
                         u.Id == currentUser.Id &&
                         u.RoomId == roomToLogin.Id)),
            Times.Once);
        _uowMock.Verify(m => m.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginToRoomAsync_OldUserAndValidRoomNameAndPassword_ShouldLoginSuccessfully()
    {
        // Arrange
        var userRepoMock = new Mock<IRepository<ApplicationUser>>();
        var roomToLogin = TestFixture.GetRoomWithUsers();
        var loginRequest = new RoomLoginDto
        {
            RoomName = roomToLogin.Name,
            RoomPassword = roomToLogin.Password,
        };
        var currentUser = TestFixture.GetCurrentUser();

        _uowMock
            .Setup(m => m.GetRepository<ApplicationUser>())
            .Returns(userRepoMock.Object);
        userRepoMock
            .Setup(m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(currentUser);
        _roomRepositoryMock
            .Setup(
                m => m.GetRoomByNameWithEverythingAsync(It.Is<string>(s => s == roomToLogin.Name)))
            .ReturnsAsync(roomToLogin);

        // Act
        var result = await _sut.LoginToRoomAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ConnectedUserNames.Should().Contain(currentUser.UserName);
        result.RoomId.Should().Be(roomToLogin.Id);
        _roomRepositoryMock.Verify(
            m =>
                m.GetRoomByNameWithEverythingAsync(
                    It.Is<string>(s => s == roomToLogin.Name)),
            Times.Once);
        _roomRepositoryMock.Verify(
            m => m.UpdateOne(
                It.Is<SqlRoom>(
                    r => r.Name == roomToLogin.Name &&
                         r.WasOnceConnectedUsers.Contains(currentUser.UserName))),
            Times.Never);
        userRepoMock.Verify(
            m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()),
            Times.Once);
        userRepoMock.Verify(
            m => m.UpdateOne(
                It.Is<ApplicationUser>(
                    u => u.UserName == currentUser.UserName &&
                         u.Id == currentUser.Id &&
                         u.RoomId == roomToLogin.Id)),
            Times.Once);
        _uowMock.Verify(m => m.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginToRoomAsync_OldUserAndInvalidPassword_ShouldLoginSuccessfully()
    {
        // Arrange
        var userRepoMock = new Mock<IRepository<ApplicationUser>>();
        var roomToLogin = TestFixture.GetRoomWithUsers();
        var loginRequest = new RoomLoginDto
        {
            RoomName = roomToLogin.Name,
            RoomPassword = string.Empty,
        };
        var currentUser = TestFixture.GetCurrentUser();

        _uowMock
            .Setup(m => m.GetRepository<ApplicationUser>())
            .Returns(userRepoMock.Object);
        userRepoMock
            .Setup(m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
            .ReturnsAsync(currentUser);
        _roomRepositoryMock
            .Setup(
                m => m.GetRoomByNameWithEverythingAsync(It.Is<string>(s => s == roomToLogin.Name)))
            .ReturnsAsync(roomToLogin);

        // Act
        var result = await _sut.LoginToRoomAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ConnectedUserNames.Should().Contain(currentUser.UserName);
        result.RoomId.Should().Be(roomToLogin.Id);
        _roomRepositoryMock.Verify(
            m =>
                m.GetRoomByNameWithEverythingAsync(
                    It.Is<string>(s => s == roomToLogin.Name)),
            Times.Once);
        _roomRepositoryMock.Verify(
            m => m.UpdateOne(
                It.Is<SqlRoom>(
                    r => r.Name == roomToLogin.Name &&
                         r.WasOnceConnectedUsers.Contains(currentUser.UserName))),
            Times.Never);
        userRepoMock.Verify(
            m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()),
            Times.Once);
        userRepoMock.Verify(
            m => m.UpdateOne(
                It.Is<ApplicationUser>(
                    u => u.UserName == currentUser.UserName &&
                         u.Id == currentUser.Id &&
                         u.RoomId == roomToLogin.Id)),
            Times.Once);
        _uowMock.Verify(m => m.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginToRoomAsync_NewUserAndInvalidPassword_ShouldNotLogin()
    {
        // Arrange
        var userRepoMock = new Mock<IRepository<ApplicationUser>>();
        var roomToLogin = TestFixture.GetEmptyRoom();
        roomToLogin.Id = 1;
        var loginRequest = new RoomLoginDto
        {
            RoomName = roomToLogin.Name,
            RoomPassword = string.Empty,
        };
        var currentUser = TestFixture.GetCurrentUser();

        _roomRepositoryMock
            .Setup(
                m => m.GetRoomByNameWithEverythingAsync(It.Is<string>(s => s == roomToLogin.Name)))
            .ReturnsAsync(roomToLogin);

        // Act
        var result = await _sut.LoginToRoomAsync(loginRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _roomRepositoryMock.Verify(
            m =>
                m.GetRoomByNameWithEverythingAsync(
                    It.Is<string>(s => s == roomToLogin.Name)),
            Times.Once);
        _roomRepositoryMock.Verify(
            m => m.UpdateOne(
                It.Is<SqlRoom>(
                    r => r.Name == roomToLogin.Name &&
                         r.WasOnceConnectedUsers.Contains(currentUser.UserName))),
            Times.Never);
        userRepoMock.Verify(
            m => m.GetOneTrackingAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>()),
            Times.Never);
        userRepoMock.Verify(
            m => m.UpdateOne(
                It.Is<ApplicationUser>(
                    u => u.UserName == currentUser.UserName &&
                         u.Id == currentUser.Id &&
                         u.RoomId == roomToLogin.Id)),
            Times.Never);
    }

    [Fact]
    public async Task AddUserResultAsync_ValidResult_ShouldAddResultToSolve()
    {
        // Arrange
        var roomSolveResultRepoMock = new Mock<IRepository<RoomSolveResult>>();
        var testRoom = TestFixture.GetRoomWithEmptySolve();
        var solveId = testRoom.Solves.First().Id;
        var request = new NewUserResultRequest
        {
            RoomId = testRoom.Id,
            SolveNumber = solveId,
            TimeInMilliseconds = 12345,
        };
        var createdResult = new RoomSolveResult
        {
            UserId = TestFixture.CurrentUserId,
            RoomSolveId = solveId,
            Time = request.TimeInMilliseconds,
        };

        _roomRepositoryMock
            .Setup(m => m.GetRoomByIdWithUsersAsync(It.Is<int>(i => i == testRoom.Id)))
            .ReturnsAsync(testRoom);
        _uowMock
            .Setup(m => m.GetRepository<RoomSolveResult>())
            .Returns(roomSolveResultRepoMock.Object);
        roomSolveResultRepoMock
            .Setup(
                m => m.CreateOneAsync(
                    It.Is<RoomSolveResult>(
                        r => r.Time == createdResult.Time &&
                             r.RoomSolveId == createdResult.RoomSolveId &&
                             r.UserId == createdResult.UserId)))
            .ReturnsAsync(createdResult);
        _userServiceMock
            .Setup(m => m.GetUserIdAsync(It.Is<string>(i => i == TestFixture.CurrentUserName)))
            .ReturnsAsync(TestFixture.CurrentUserId);

        // Act
        var result = await _sut.AddUserResultAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.RoomName.Should().BeEquivalentTo(testRoom.Name);
        result.Model.RoomSolveId.Should().Be(solveId);
        result.Model.UserName.Should().BeEquivalentTo(TestFixture.CurrentUserName);
        result.Model.Time.Should().Be(request.TimeInMilliseconds);
        _roomRepositoryMock.Verify(
            m =>
                m.GetRoomByIdWithUsersAsync(It.Is<int>(i => i == testRoom.Id)),
            Times.Once);
        roomSolveResultRepoMock.Verify(
            m => m.CreateOneAsync(
                It.Is<RoomSolveResult>(
                    r => r.Time == createdResult.Time &&
                         r.RoomSolveId == createdResult.RoomSolveId &&
                         r.UserId == createdResult.UserId)),
            Times.Once);
        _uowMock.Verify(m => m.SaveAsync(), Times.Once);
    }
}
