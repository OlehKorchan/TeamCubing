using System.Security.Claims;

namespace TeamCubing.Domain.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string UserName(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
