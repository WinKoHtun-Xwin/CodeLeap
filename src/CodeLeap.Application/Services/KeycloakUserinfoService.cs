using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CodeLeap.Application.Services
{
    public class KeycloakUserinfoService(IHttpContextAccessor httpContextAccessor, ILogger<KeycloakUserinfoService> logger) : IKeycloakUserinfoService
    {
        public string? UserId
        {
            get
            {
                var context = httpContextAccessor.HttpContext;
                logger.LogInformation("KeycloakUserinfoService: Accessing UserId. HttpContext present: {IsContextPresent}, User Authenticated: {IsAuthenticated}",
                    context != null,
                    context?.User?.Identity?.IsAuthenticated ?? false);

                return context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context?.User?.FindFirst("sub")?.Value;
            }
        }

        public string? Username => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                                   ?? httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value;

        public string? Email => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                                ?? httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

        public string? FirstName => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.GivenName)?.Value
                                    ?? httpContextAccessor.HttpContext?.User?.FindFirst("given_name")?.Value;

        public string? LastName => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Surname)?.Value
                                   ?? httpContextAccessor.HttpContext?.User?.FindFirst("family_name")?.Value;

        public ClaimsPrincipal? ClaimsPrincipal => httpContextAccessor.HttpContext?.User;
    }
}
