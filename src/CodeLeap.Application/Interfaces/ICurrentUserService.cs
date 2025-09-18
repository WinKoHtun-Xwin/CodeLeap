using System.Security.Claims;

namespace CodeLeap.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? GetUserId();
    string? GetUsername();
    string? GetUserRole();
    bool IsAuthenticated();
    ClaimsPrincipal? GetCurrentUser();
}
