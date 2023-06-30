using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamCubing.BLL.Interfaces;
using TeamCubing.DAL.Interfaces;
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
    private readonly IUserRepository _userRepository;

    public AuthService(
        IJwtGenerator jwtGenerator,
        ILogger<AuthService> logger,
        IOptions<Settings> settings,
        IUserRepository userRepository)
    {
        _jwtGenerator = jwtGenerator;
        _logger = logger;
        _userRepository = userRepository;
        _jwtSettings = settings.Value.JwtSettings;
    }

    public async Task<LoginResponseModel> LoginAsync(LoginRequestModel request)
    {
        var response = new LoginResponseModel();

        if (await ValidateLoginAsync(request, response))
        {
            response.IsSuccess = true;
            response.Username = request.Login;
            response.Token =
                _jwtGenerator.GenerateToken(await _userRepository.ReadByNameAsync(request.Login));

            response.ExpiresIn = _jwtSettings.ExpiresInHours;

            _logger.LogInformation("Sign in for user {Username} successful", response.Username);
        }
        else
        {
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

        if (!await ValidateRegistrationAsync(request, response))
        {
            return response;
        }

        var hashedPassword = HashPassword(request.Password, out var salt);
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            PasswordHash = hashedPassword,
            PasswordSalt = salt,
        };

        try
        {
            var inserted = await _userRepository.InsertAsync(user);

            _logger.LogInformation(
                "User {Username} successfully registered",
                request.UserName);

            response.IsSuccess = true;
            response.Username = user.UserName;
            response.Token = _jwtGenerator.GenerateToken(inserted);

            response.ExpiresIn = _jwtSettings.ExpiresInHours;
        }
        catch (Exception e)
        {
            response.ErrorMessage = "Registration failed, try again";

            _logger.LogError("Registration error: {Description}", e.Message);
        }

        return response;
    }

    private async Task<bool> ValidateRegistrationAsync(
        RegisterRequestModel requestModel,
        BaseResponse response)
    {
        if (requestModel.Password.Length < 5)
        {
            response.ErrorMessage += "Password should contain at least 5 symbols";

            return false;
        }

        var user = await _userRepository.ReadByNameAsync(requestModel.UserName);

        if (user is not null)
        {
            response.ErrorMessage += "Provided username already registered";

            return false;
        }

        return true;
    }

    private static string HashPassword(string password, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(128 / 8);

        var hashed = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

        return hashed;
    }

    private async Task<bool> ValidateLoginAsync(LoginRequestModel request, BaseResponse response)
    {
        var user = await _userRepository.ReadByNameAsync(request.Login);

        if (user is null)
        {
            response.ErrorMessage = "Invalid username";

            return false;
        }

        var hashedRequestPassword= Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password: request.Password!,
                salt: user.PasswordSalt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

        if (hashedRequestPassword != user.PasswordHash)
        {
            response.ErrorMessage = "Invalid password";

            return false;
        }

        return true;
    }
}
