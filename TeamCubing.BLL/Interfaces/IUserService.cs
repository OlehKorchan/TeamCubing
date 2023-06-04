using TeamCubing.DAL.Models;

namespace TeamCubing.BLL.Interfaces;

public interface IUserService
{
    Task<ApplicationUser> GetUserByNameAsync(string userName);
    Task<string> GetUserIdAsync(string userName);
    Task<string> GetUserNameAsync(string userId);
}
