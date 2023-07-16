using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.ResponseModels;

public class RoomDisplayDataResponse
{
    public string Id { get; set; }

    public string RoomName { get; set; }

    public RoomPuzzle Puzzle { get; set; }

    public string AdministratorName { get; set; }

    public int ConnectedUsersCount { get; set; }

    public int MaxUsersCount { get; set; }

    public bool IsOpen { get; set; }
}
