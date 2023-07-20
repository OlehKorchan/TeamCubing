using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCubing.BLL.Helpers;
using TeamCubing.BLL.Interfaces;
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
    public async Task<IActionResult> GetUserResults()
    {
        return Ok(await _accountService.GetUserStatistics());
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await _accountService.GetAllAsync());
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterRequestModel registerModel)
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
    public async Task<IActionResult> LoginAsync(LoginRequestModel loginModel)
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
