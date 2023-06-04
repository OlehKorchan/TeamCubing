using TeamCubing.BLL.Models;

namespace TeamCubing.BLL.Interfaces;

public interface ISessionService
{
    Task<UserSessionDto> GetUserSessionAsync(string sessionName);
    Task<List<UserSessionDto>> GetUserSessionsAsync();
    Task<UserSessionDto> NewSessionAsync(UserSessionDto sessionDto);
    Task<SolveDto> SaveSolveAsync(SolveDto solve);
    Task RemoveSessionAsync(int sessionId);
    Task RemoveSolveAsync(int solveId);
    Task ClearSessionAsync(int sessionId);
}
