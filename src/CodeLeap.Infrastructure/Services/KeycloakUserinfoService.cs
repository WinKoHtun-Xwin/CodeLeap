using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;

namespace CodeLeap.Infrastructure.Services;

public class KeycloakUserinfoService : IKeycloakUserinfoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Microsoft.Extensions.Logging.ILogger<KeycloakUserinfoService> _logger;

    public KeycloakUserinfoService(IHttpContextAccessor httpContextAccessor, Microsoft.Extensions.Logging.ILogger<KeycloakUserinfoService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                _logger.LogWarning("KeycloakUserinfoService: HttpContext or User is null");
                return null;
            }

            // Debug logging
            _logger.LogInformation("KeycloakUserinfoService: Inspecting claims for user...");
            foreach (var claim in user.Claims)
            {
                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            var userId = GetClaim(ClaimTypes.NameIdentifier)
                         ?? GetClaim(JwtRegisteredClaimNames.Sub)
                         ?? GetClaim("preferred_username")
                         ?? GetClaim(ClaimTypes.Email)
                         ?? GetClaim(ClaimTypes.Name);

            _logger.LogInformation("KeycloakUserinfoService: Resolved UserId = {UserId}", userId);
            return userId;
        }
    }

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
