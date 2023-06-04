namespace TeamCubing.DAL.Models;

public class Session
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int? RoomId { get; set; }

    public Room Room { get; set; }

    public string UserId { get; set; }

    public ApplicationUser User { get; set; }

    public List<Solve> Solves { get; set; }
}
