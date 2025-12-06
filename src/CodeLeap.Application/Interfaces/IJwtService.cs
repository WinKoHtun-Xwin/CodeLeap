using CodeLeap.Application.DTOs.Auth;

namespace CodeLeap.Application.Interfaces;

public interface IJwtService
{
    Task<GetAuthDto?> GenerateToken(string userId, string username, List<string> roles);
    Task<GetAuthDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
}
