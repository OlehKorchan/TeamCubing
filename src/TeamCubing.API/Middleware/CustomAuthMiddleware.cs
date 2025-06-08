using System.IdentityModel.Tokens.Jwt;
using TeamCubing.Domain.DTO;

namespace TeamCubing.API.Middleware;

public class CustomAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomAuthMiddleware> _logger;
    private readonly UserContext _userContext;

    public CustomAuthMiddleware(
        RequestDelegate next,
        ILogger<CustomAuthMiddleware> logger,
        UserContext userContext)
    {
        _next = next;
        _logger = logger;
        _userContext = userContext;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            var userName = context.User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;

            _userContext.UserName = userName;
        }

        await _next(context);
    }
}