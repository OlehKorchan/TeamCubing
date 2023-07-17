using TeamCubing.Domain.Models;

namespace TeamCubing.DAL.Interfaces;

public interface IUserRepository : ICrudRepository<ApplicationUser>
{
    Task<ApplicationUser> ReadByNameAsync(string userName);

    Task<List<ApplicationUser>> ReadAllAsync();

    Task InsertSolveAsync(UserSolve solve, string userName);
}
