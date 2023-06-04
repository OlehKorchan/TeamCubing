using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TeamCubing.API.Helpers;
using TeamCubing.API.Models;
using TeamCubing.API.Settings;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Settings;
using TeamCubing.DAL.Models;

namespace TeamCubing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IJwtGenerator _jwtGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AccountController> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtGenerator jwtGenerator,
        ILogger<AccountController> logger,
        IOptions<JwtSettings> jwtSettings
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtGenerator = jwtGenerator;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(
        RegisterRequestModel registerModel)
    {
        var responseModel =
            new RegisterResponseModel();

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = registerModel.UserName,
                Email = registerModel.UserName
            };

            var registerResult =
                await _userManager.CreateAsync(
                    user, registerModel.Password);

            if (registerResult.Succeeded)
            {
                _logger.LogInformation(
                    "User {Username} successfully registered",
                    registerModel.UserName);

                await _signInManager.SignInAsync(
                    user, false);

                responseModel.Username = user.UserName;
                responseModel.Token = _jwtGenerator.GenerateToken(
                    await _userManager.FindByNameAsync(
                        registerModel.UserName));

                responseModel.ExpiresIn = _jwtSettings.ExpiresInHours;
            }
            else
            {
                _logger.LogError(
                    "User {Username} registration failed",
                    registerModel.UserName);

                foreach (var error in registerResult.Errors)
                {
                    responseModel.Errors.Add(error.Description);
                    _logger.LogError(
                        "Registration error: {Description}",
                        error.Description);
                }
            }
        }
        else
        {
            ModelErrorsHelper.PutModelStateErrorsToResponseModel(
                ModelState, responseModel);

            _logger.LogError(
                "Validation of the model failed:" +
                "\n{Model}\nwith model errors:\n{Errors}",
                JsonSerializer.Serialize(registerModel),
                JsonSerializer.Serialize(responseModel.Errors)
            );
        }

        return Ok(responseModel);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        LoginRequestModel loginModel)
    {
        var responseModel = new LoginResponseModel();

        if (ModelState.IsValid)
        {
            var loginResult = await _signInManager
                .PasswordSignInAsync(
                    loginModel.Login,
                    loginModel.Password,
                    false,
                    false);

            if (loginResult.Succeeded)
            {
                responseModel.Username = loginModel.Login;
                responseModel.Token = _jwtGenerator.GenerateToken(
                    await _userManager.FindByNameAsync(
                        loginModel.Login));

                responseModel.ExpiresIn = _jwtSettings.ExpiresInHours;
                _logger.LogInformation(
                    "Sign in for user {Username} successful",
                    responseModel.Username);
            }
            else
            {
                responseModel.Errors.Add(ErrorMessages.LoginError);
                _logger.LogError(
                    "User {Login} sign in failed with errors:" +
                    "\n{Errors}",
                    loginModel.Login,
                    responseModel.Errors);
            }
        }
        else
        {
            ModelErrorsHelper.PutModelStateErrorsToResponseModel(
                ModelState, responseModel);

            _logger.LogError(
                "User {Login} sign in failed with errors:" +
                "\n{Errors}",
                loginModel.Login,
                responseModel.Errors);
        }

        return Ok(responseModel);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogDebug("User has been logged out");

        return Ok();
    }
}