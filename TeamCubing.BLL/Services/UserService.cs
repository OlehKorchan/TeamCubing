using TeamCubing.BLL.Interfaces;
using TeamCubing.DAL.Interfaces;
using TeamCubing.DAL.Models;

namespace TeamCubing.BLL.Services;

public class UserService : IUserService
{
    private readonly IRepository<ApplicationUser> _userRepository;

    public UserService(IUnitOfWork unitOfWork)
    {
        _userRepository = unitOfWork.GetRepository<ApplicationUser>();
    }

    public Task<ApplicationUser> GetUserByNameAsync(string userName)
    {
        return _userRepository.GetOneAsync(u => u.UserName == userName);
    }

    public async Task<string> GetUserIdAsync(string userName)
    {
        return (await GetUserByNameAsync(userName))?.Id;
    }

    public async Task<string> GetUserNameAsync(string userId)
    {
        return (await _userRepository.GetOneAsync(u => u.Id == userId)).UserName;
    }
}
