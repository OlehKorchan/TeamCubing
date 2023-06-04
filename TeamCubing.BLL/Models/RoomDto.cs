using TeamCubing.DAL.Models;

namespace TeamCubing.BLL.Models;

public class RoomDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<string> WasOnceConnectedUsers { get; set; }

    public List<string> ConnectedUserNames { get; set; } = new();

    public List<RoomSolve> Solves { get; set; } = new();
}
