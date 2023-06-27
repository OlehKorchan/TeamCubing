using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamCubing.BLL.Helpers;
using TeamCubing.BLL.Interfaces;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.RequestModels;
using TeamCubing.Domain.ResponseModels;
using TeamCubing.Domain.Settings;

namespace TeamCubing.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IJwtGenerator _jwtGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtGenerator jwtGenerator,
        ILogger<AuthService> logger,
        IOptions<Settings> settings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtGenerator = jwtGenerator;
        _logger = logger;
        _jwtSettings = settings.Value.JwtSettings;
    }

    public async Task<LoginResponseModel> LoginAsync(LoginRequestModel request)
    {
        var response = new LoginResponseModel();


        var loginResult = new SignInResult();
        try
        {
            loginResult = await _signInManager
                .PasswordSignInAsync(
                    request.Login,
                    request.Password,
                    false,
                    false);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error while signing in user {UserName}, message: {Message}",
                request.Login,
                e.Message);
        }


        if (loginResult.Succeeded)
        {
            response.IsSuccess = true;
            response.Username = request.Login;
            response.Token =
                _jwtGenerator.GenerateToken(await _userManager.FindByNameAsync(request.Login));

            response.ExpiresIn = _jwtSettings.ExpiresInHours;
            _logger.LogInformation(
                "Sign in for user {Username} successful",
                response.Username);
        }
        else
        {
            response.ErrorMessage = "Invalid username or password";
            _logger.LogError(
                "User {Login} sign in failed with errors:" +
                "\n{Errors}",
                request.Login,
                response.ErrorMessage);
        }

        return response;
    }

    public async Task<RegisterResponseModel> RegisterAsync(RegisterRequestModel request)
    {
        var response = new RegisterResponseModel();

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.UserName,
        };

        var registerResult = await _userManager.CreateAsync(user, request.Password);

        if (registerResult.Succeeded)
        {
            _logger.LogInformation(
                "User {Username} successfully registered",
                request.UserName);

            await _signInManager.SignInAsync(
                user,
                false);

            response.IsSuccess = true;
            response.Username = user.UserName;
            response.Token =
                _jwtGenerator.GenerateToken(await _userManager.FindByNameAsync(request.UserName));

            response.ExpiresIn = _jwtSettings.ExpiresInHours;
        }
        else
        {
            _logger.LogError(
                "User {Username} registration failed",
                request.UserName);

            ModelErrorsHelper.MapToSingleError(registerResult.Errors, response);
            _logger.LogError(
                "Registration error: {Description}",
                response.ErrorMessage);
        }

        return response;
    }

    public Task LogoutAsync()
    {
        return _signInManager.SignOutAsync();
    }
}
