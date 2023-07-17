using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;
using TeamCubing.Domain.ResponseModels;

namespace TeamCubing.BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResponseModel> LoginAsync(LoginRequestModel request);
    Task<RegisterResponseModel> RegisterAsync(RegisterRequestModel request);
    Task<List<ApplicationUser>> GetAllAsync();
}
