using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCubing.API.Models;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Models;

namespace TeamCubing.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SessionsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService, IMapper mapper)
    {
        _sessionService = sessionService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<SessionResponseModel>>> GetAsync()
    {
        var sessions = await _sessionService.GetUserSessionsAsync();

        return Ok(_mapper.Map<List<SessionResponseModel>>(sessions));
    }

    [HttpGet("{sessionName}")]
    public async Task<ActionResult<SessionResponseModel>> GetAsync(string sessionName)
    {
        var session = await _sessionService.GetUserSessionAsync(sessionName);

        return Ok(_mapper.Map<SessionResponseModel>(session));
    }

    [HttpPost]
    public async Task<ActionResult<SessionResponseModel>> PostAsync(SessionRequestModel request)
    {
        var createdSession = await _sessionService.NewSessionAsync(
            new UserSessionDto
            {
                Name = request.Name,
            });

        return _mapper.Map<SessionResponseModel>(createdSession);
    }

    [HttpPost("solve")]
    public async Task<ActionResult<SolveResponseModel>> PostAsync(SolveRequestModel solveRequest)
    {
        var solve = _mapper.Map<SolveDto>(solveRequest);

        var response = await _sessionService.SaveSolveAsync(solve);

        return Ok(_mapper.Map<SolveResponseModel>(response));
    }

    [HttpDelete("{sessionId:int}")]
    public async Task<IActionResult> DeleteAsync(int sessionId)
    {
        await _sessionService.RemoveSessionAsync(sessionId);

        return Ok();
    }

    [HttpDelete("clear/{sessionId:int}")]
    public async Task<IActionResult> ClearSessionAsync(int sessionId)
    {
        await _sessionService.ClearSessionAsync(sessionId);

        return Ok();
    }

    [HttpDelete("solve/{solveId:int}")]
    public async Task<ActionResult<bool>> DeleteSolveAsync(int solveId)
    {
        await _sessionService.RemoveSolveAsync(solveId);

        return Ok(true);
    }
}
