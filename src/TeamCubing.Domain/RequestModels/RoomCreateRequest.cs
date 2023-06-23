using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.RequestModels;

public class RoomCreateRequest
{
    public string RoomName { get; set; }

    public string RoomPassword { get; set; }

    public RoomSettings Settings { get; set; }
}
