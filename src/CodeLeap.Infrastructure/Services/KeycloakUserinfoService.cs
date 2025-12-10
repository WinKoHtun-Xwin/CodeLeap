using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace CodeLeap.Infrastructure.Services;

public class KeycloakUserinfoService : IKeycloakUserinfoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public KeycloakUserinfoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => GetClaim(ClaimTypes.NameIdentifier)
                             ?? GetClaim(JwtRegisteredClaimNames.Sub)
                             ?? GetClaim("preferred_username")
                             ?? GetClaim(ClaimTypes.Email)
                             ?? GetClaim(ClaimTypes.Name);

    public string? Username => GetClaim(ClaimTypes.Name) ?? GetClaim("preferred_username");

    public string? Email => GetClaim(ClaimTypes.Email) ?? GetClaim("email");

    public string? FirstName => GetClaim(ClaimTypes.GivenName) ?? GetClaim("given_name");

    public string? LastName => GetClaim(ClaimTypes.Surname) ?? GetClaim("family_name");

    public ClaimsPrincipal? ClaimsPrincipal => _httpContextAccessor.HttpContext?.User;

    private string? GetClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }
}
