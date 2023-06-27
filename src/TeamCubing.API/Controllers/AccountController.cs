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
    private readonly IAuthService _authService;

    public AccountController(
        ILogger<AccountController> logger,
        IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterRequestModel registerModel)
    {
        var responseModel =
            new RegisterResponseModel();

        if (ModelState.IsValid)
        {
            responseModel = await _authService.RegisterAsync(registerModel);
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
            responseModel = await _authService.LoginAsync(loginModel);
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

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        _logger.LogDebug("User has been logged out");

        return Ok();
    }
}
