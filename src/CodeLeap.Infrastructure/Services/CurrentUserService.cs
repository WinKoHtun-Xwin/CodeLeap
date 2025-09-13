using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CodeLeap.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated is true)
        {
            return user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                   user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        return null;
    }

    public string? GetUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated is true)
        {
            return user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ??
                   user.FindFirst(ClaimTypes.Name)?.Value;
        }
        return null;
    }

    public string? GetUserRole()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated is true)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }
        return null;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated is true;
    }

    public ClaimsPrincipal? GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User;
    }
}
