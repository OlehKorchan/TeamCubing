namespace TeamCubing.BLL.Tests.Helpers;

public static class TestFixture
{
    public const string TestRoomName = "ROOMNAME228";
    public const string TestRoomPassword = "ROOMPASSWORD228";
    public const int TestRoomId = 228;
    public const string CurrentUserName = "UserName228";
    public const string CurrentUserId = "UserId228";

    public static ApplicationUser GetCurrentUser()
    {
        return new ApplicationUser
        {
            UserName = CurrentUserName,
            Id = CurrentUserId,
        };
    }

    public static SqlRoom GetEmptyRoom()
    {
        return new SqlRoom
        {
            Name = TestRoomName,
            Password = TestRoomPassword,
        };
    }

    public static RoomLoginDto GetRoomLoginRequest()
    {
        return new RoomLoginDto
        {
            RoomName = TestRoomName,
            RoomPassword = TestRoomPassword,
        };
    }

    public static SqlRoom GetRoomWithEmptySolve()
    {
        var room = GetRoomWithUsers();

        room.Solves.Add(GetBaseRoomSolve());

        return room;
    }

    public static SqlRoom GetRoomWithFinishedSolve()
    {
        var roomWithUsers = GetRoomWithUsers();

        roomWithUsers.Solves.Add(GetFinishedRoomSolve());

        return roomWithUsers;
    }

    public static SqlRoom GetRoomWithUsers()
    {
        var room = GetEmptyRoom();

        room.Id = TestRoomId;
        room.Users = new List<ApplicationUser> { GetCurrentUser() };
        room.WasOnceConnectedUsers = CurrentUserName + ",";

        return room;
    }

    public static RoomSolve GetFinishedRoomSolve()
    {
        var baseSolve = GetBaseRoomSolve();
        baseSolve.Results = new List<RoomSolveResult>
        {
            new()
            {
                Id = 1,
                Time = 10000,
                UserId = CurrentUserId,
                User = GetCurrentUser(),
            },
        };

        return baseSolve;
    }

    public static RoomSolve GetBaseRoomSolve()
    {
        return new RoomSolve
        {
            Id = 1,
            Scramble = "R U R U L U L U",
            RoomId = TestRoomId,
            SolveNumber = 1,
            StartTime = DateTime.UtcNow,
        };
    }
}
