using System.Security.Claims;
using TeamCubing.Domain.Models;

namespace TeamCubing.DAL.Interfaces;

public interface IUserRepository : ICrudRepository<ApplicationUser>
{
    Task<ApplicationUser> ReadByNameAsync(string userName);
}
