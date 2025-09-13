using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CodeLeap.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
               principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetUsername(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ??
               principal.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static string? GetUserRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Role)?.Value;
    }

    public static IEnumerable<string> GetUserRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    public static string GetUserIdOrThrow(this ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
