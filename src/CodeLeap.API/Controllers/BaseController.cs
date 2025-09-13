using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CodeLeap.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current user ID from JWT token
    /// </summary>
    protected string? GetCurrentUserId()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                   User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        return null;
    }

    /// <summary>
    /// Gets the current username from JWT token
    /// </summary>
    protected string? GetCurrentUsername()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ??
                   User.FindFirst(ClaimTypes.Name)?.Value;
        }
        return null;
    }

    /// <summary>
    /// Gets the current user role from JWT token
    /// </summary>
    protected string? GetCurrentUserRole()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
        return null;
    }

    /// <summary>
    /// Checks if current user is authenticated
    /// </summary>
    protected bool IsCurrentUserAuthenticated()
    {
        return User?.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Gets the current user ID and throws UnauthorizedAccessException if not authenticated
    /// </summary>
    protected string GetCurrentUserIdOrThrow()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated or user ID not found in token");
        }
        return userId;
    }
}
