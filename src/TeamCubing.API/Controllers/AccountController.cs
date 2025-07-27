using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCubing.BLL.Helpers;
using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;
using TeamCubing.Domain.ResponseModels;

namespace TeamCubing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountService _accountService;

    public AccountController(
        ILogger<AccountController> logger,
        IAccountService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    [HttpGet("results")]
    [Authorize]
    [ProducesResponseType<UserStatisticsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserStatisticsResponse>> GetUserResults()
    {
        return Ok(await _accountService.GetUserStatistics());
    }

    /// <summary>
    /// Get all application users for admin purposes.
    /// </summary>
    /// <returns>List of application users.</returns>
    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType<List<ApplicationUser>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ApplicationUser>>> GetAllUsers()
    {
        return Ok(await _accountService.GetAllAsync());
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseModel>> Register(RegisterRequestModel registerModel)
    {
        var responseModel =
            new RegisterResponseModel();

        if (ModelState.IsValid)
        {
            responseModel = await _accountService.RegisterAsync(registerModel);
        }
        else
        {
            ModelErrorsHelper.PutModelStateErrorsToResponseModel(ModelState, responseModel);

            _logger.LogError(
                "Validation of the model failed:" +
                "\n{Model}\nwith model errors:\n{Errors}",
                JsonSerializer.Serialize(registerModel),
                JsonSerializer.Serialize(responseModel.ErrorMessage));
        }

        return Ok(responseModel);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseModel>> Login(LoginRequestModel loginModel)
    {
        var responseModel = new LoginResponseModel();

        if (ModelState.IsValid)
        {
            responseModel = await _accountService.LoginAsync(loginModel);
        }
        else
        {
            ModelErrorsHelper.PutModelStateErrorsToResponseModel(ModelState, responseModel);

            _logger.LogError(
                "User {Login} sign in failed with errors:" +
                "\n{Errors}",
                loginModel.Login,
                responseModel.ErrorMessage);
        }

        return Ok(responseModel);
    }
}