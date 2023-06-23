using Newtonsoft.Json;

namespace TeamCubing.Domain.Models;

public class RoomSettings
{
    [JsonProperty("puzzle")]
    public RoomPuzzle Puzzle { get; set; }

    [JsonProperty("isOpen")]
    public bool IsOpen { get; set; }

    [JsonProperty("enableSolveTimeLimit")]
    public bool EnableSolveTimeLimit { get; set; }

    [JsonProperty("usersLimit")]
    public int UsersLimit { get; set; }
}
