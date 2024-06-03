using Microsoft.AspNetCore.Mvc;
using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.Models;

namespace TeamCubing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScrambleController : ControllerBase
{
    private readonly IScramblerService _scramblerService;

    public ScrambleController(IScramblerService scramblerService)
    {
        _scramblerService = scramblerService;
    }

    [HttpGet]
    public IActionResult GetScrambleAsync(RoomPuzzle puzzle)
    {
        return Ok(_scramblerService.GenerateScrambleWithImage(puzzle));
    }
}