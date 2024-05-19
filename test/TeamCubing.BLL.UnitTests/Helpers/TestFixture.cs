using System.Security.Claims;
using TeamCubing.Domain.DTO;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;

namespace TeamCubing.BLL.Tests.Helpers;

public static class TestFixture
{
    public const string TestRoomName = "ROOMNAME228";
    public const string TestRoomPassword = "ROOMPASSWORD228";
    public const string TestRoomId = "228";
    public const string CurrentUserName = "UserName228";
    public const string CurrentUserId = "UserId228";

    public static Room GetEmptyRoom()
    {
        return new Room
        {
            Name = TestRoomName,
            Password = TestRoomPassword,
            Settings = new RoomSettings
            {
                IsOpen = false,
                UsersLimit = 3,
                EnableSolveTimeLimit = true,
            },
        };
    }

    public static RoomLoginRequest GetRoomLoginRequest()
    {
        return new RoomLoginRequest
        {
            RoomName = TestRoomName,
            RoomPassword = TestRoomPassword,
        };
    }

    public static RoomCreateRequest GetRoomCreateRequest()
    {
        return new RoomCreateRequest
        {
            RoomName = TestRoomName,
            RoomPassword = TestRoomPassword,
        };
    }

    public static Room RoomWithEmptySolve()
    {
        var room = GetRoomWithUsers();

        room.Solves.Add(BaseRoomSolve());

        return room;
    }

    public static Room RoomWithFinishedSolve()
    {
        var roomWithUsers = GetRoomWithUsers();

        roomWithUsers.Solves.Add(GetFinishedRoomSolve());

        return roomWithUsers;
    }

    public static Room GetRoomWithUsers()
    {
        var room = GetEmptyRoom();

        room.Id = TestRoomId;
        room.ConnectedUserNames = new List<string> { CurrentUserName };
        room.WasOnceConnectedUserNames = new List<string> { CurrentUserName };

        return room;
    }

    public static Solve GetFinishedRoomSolve()
    {
        var baseSolve = BaseRoomSolve();
        baseSolve.Results = new List<SolveResult>
        {
            new()
            {
                Time = 10000,
                UserName = CurrentUserName,
            },
        };

        return baseSolve;
    }

    public static Solve BaseRoomSolve()
    {
        return new Solve
        {
            Scramble = "R U R U L U L U",
            SolveNumber = 1,
            StartTime = DateTime.UtcNow,
        };
    }

    public static ScrambleWithImage ScrambleWithImage()
    {
        return new ScrambleWithImage
        {
            Scramble = "R U R U L U L U",
            Image = new PuzzleImage()
        };
    }

    public static ApplicationUser GetCurrentUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = CurrentUserName,
            PasswordHash = "HASH",
        };
    }

    public static ClaimsPrincipal GetCurrentClaimsPrincipal()
    {
        return new ClaimsPrincipal(
            new[]
            {
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, CurrentUserName),
                    }),
            });
    }
}
