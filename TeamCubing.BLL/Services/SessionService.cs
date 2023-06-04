using AutoMapper;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Models;
using TeamCubing.DAL.Interfaces;
using TeamCubing.DAL.Models;

namespace TeamCubing.BLL.Services;

public class SessionService : ISessionService
{
    private readonly IMapper _mapper;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<Solve> _solveRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationUser _user;
    private readonly IUserService _userService;

    public SessionService(
        IUnitOfWork unitOfWork,
        ApplicationUser user,
        IMapper mapper,
        IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _user = user;
        _mapper = mapper;
        _userService = userService;
        _sessionRepository = _unitOfWork.GetRepository<Session>();
        _solveRepository = _unitOfWork.GetRepository<Solve>();
    }

    public async Task<UserSessionDto> GetUserSessionAsync(string sessionName)
    {
        var userId = await _userService.GetUserIdAsync(_user.UserName);
        var session = await _sessionRepository.GetOneAsync(
            s => s.Name == sessionName && s.UserId == userId);

        await LoadSessionsSolvesAsync(new List<Session> { session });

        return _mapper.Map<UserSessionDto>(session);
    }

    public async Task<List<UserSessionDto>> GetUserSessionsAsync()
    {
        var userId = await _userService.GetUserIdAsync(_user.UserName);
        var sessions = await _sessionRepository.GetManyAsync(
            s => s.UserId == userId);

        if (sessions.Count == 0)
        {
            sessions.Add(
                await _sessionRepository.CreateOneAsync(
                    new Session
                    {
                        UserId = userId,
                        Name = "session1",
                    }));

            await _unitOfWork.SaveAsync();
        }

        await LoadSessionsSolvesAsync(sessions);

        return _mapper.Map<List<UserSessionDto>>(sessions);
    }

    public async Task<UserSessionDto> NewSessionAsync(UserSessionDto sessionDto)
    {
        var userId = await _userService.GetUserIdAsync(_user.UserName);

        var session = _mapper.Map<Session>(sessionDto);

        session.UserId = userId;

        var response = await _sessionRepository.CreateOneAsync(session);
        await _unitOfWork.SaveAsync();

        return _mapper.Map<UserSessionDto>(response);
    }

    public async Task<SolveDto> SaveSolveAsync(SolveDto solve)
    {
        var createdSolve = await _solveRepository.CreateOneAsync(_mapper.Map<Solve>(solve));

        await _unitOfWork.SaveAsync();

        return _mapper.Map<SolveDto>(createdSolve);
    }

    public async Task RemoveSessionAsync(int sessionId)
    {
        var userId = await _userService.GetUserIdAsync(_user.UserName);
        await _sessionRepository.DeleteOneAsync(s => s.Id == sessionId && s.UserId == userId);

        _unitOfWork.SaveAsync().GetAwaiter().GetResult();
    }

    public async Task RemoveSolveAsync(int solveId)
    {
        await _solveRepository.DeleteOneAsync(s => s.Id == solveId);
        _unitOfWork.SaveAsync().GetAwaiter().GetResult();
    }

    public async Task ClearSessionAsync(int sessionId)
    {
        var userId = (await _userService.GetUserByNameAsync(_user.UserName)).Id;
        var userSession = await _sessionRepository.GetOneAsync(
            s => s.Id == sessionId && s.UserId == userId);

        _solveRepository.DeleteMany(s => s.SessionId == userSession.Id);
        _unitOfWork.SaveAsync().GetAwaiter().GetResult();
    }

    private async Task LoadSessionsSolvesAsync(List<Session> sessions)
    {
        var sessionsIds = sessions.Select(s => s.Id);
        var sessionsSolves =
            await _solveRepository.GetManyAsync(
                s => sessionsIds.Contains(s.SessionId));

        sessions.ForEach(
            s => s.Solves = sessionsSolves
                .Where(solve => solve.SessionId == s.Id)
                .ToList());
    }
}
