using TeamCubing.DAL.Models;

namespace TeamCubing.BLL.Interfaces;

public interface IJwtGenerator
{
    string GenerateToken(ApplicationUser user);
}