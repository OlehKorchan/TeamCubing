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

    public static Room GetRoomWithEmptySolve()
    {
        var room = GetRoomWithUsers();

        room.Solves.Add(GetBaseRoomSolve());

        return room;
    }

    public static Room GetRoomWithFinishedSolve()
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
        var baseSolve = GetBaseRoomSolve();
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

    public static Solve GetBaseRoomSolve()
    {
        return new Solve
        {
            Scramble = "R U R U L U L U",
            SolveNumber = 1,
            StartTime = DateTime.UtcNow,
        };
    }
}
