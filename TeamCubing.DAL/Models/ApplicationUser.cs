using Microsoft.AspNetCore.Identity;

namespace TeamCubing.DAL.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }

    public string SecondName { get; set; }

    public int? RoomId { get; set; }

    public Room Room { get; set; }

    public List<RoomSolveResult> RoomSolvesResults { get; set; }

    public List<Session> Sessions { get; set; }
}
