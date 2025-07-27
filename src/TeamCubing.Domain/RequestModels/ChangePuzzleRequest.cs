using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.RequestModels;

public class ChangePuzzleRequest
{
    public string RoomName { get; set; }

    public Puzzle Puzzle { get; set; }
}
