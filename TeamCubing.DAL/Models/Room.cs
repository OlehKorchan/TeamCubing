namespace TeamCubing.DAL.Models;

public class Room
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string WasOnceConnectedUsers { get; set; }

    public string Password { get; set; }

    public List<RoomSolve> Solves { get; set; } = new();

    public List<ApplicationUser> Users { get; set; } = new();
}
