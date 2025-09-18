using CodeLeap.Application.DTOs.Auth;

namespace CodeLeap.Application.Interfaces;

public interface IJwtService
{
    Task<GetAuthDto?> GenerateToken(string userId, string username, string role);
    string? ValidateToken(string token);

    Task<GetAuthDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
}
