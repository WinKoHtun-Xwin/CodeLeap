using System.Security.Claims;

namespace CodeLeap.Application.Interfaces;

public interface IKeycloakUserinfoService
{
    string? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    ClaimsPrincipal? ClaimsPrincipal { get; }
}
