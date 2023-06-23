using Microsoft.AspNetCore.Identity;

namespace TeamCubing.Domain.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }

    public string SecondName { get; set; }

    public string LastRoomName { get; set; }
}
