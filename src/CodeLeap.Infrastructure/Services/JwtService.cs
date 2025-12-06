using CodeLeap.Application.DTOs.Auth;
using CodeLeap.Application.Interfaces;
using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CodeLeap.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<UserEntity> _userManager;

    public JwtService(
        IConfiguration config, 
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<UserEntity> userManager)
    {
        _config = config;
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
    }

    public async Task<GetAuthDto?> GenerateToken(string userId, string username, List<string> roles)
    {
        var accessKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Access_Key"]!));
        var creds = new SigningCredentials(accessKey, SecurityAlgorithms.HmacSha256);

        // Create claims list
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username)
        };

        // Add each role as a separate claim
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var accessExpiry = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!));
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: accessExpiry,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Create refresh token
        var refreshToken = new RefreshTokenEntity
        {
            UserId = userId,
            Token = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!))
        };

        await _refreshTokenRepository.CreateRefreshTokenAsync(refreshToken);

        return new GetAuthDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = accessExpiry
        };
    }

    public string? ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var accessKey = Encoding.UTF8.GetBytes(_config["Jwt:Access_Key"]!);

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(accessKey),
                ValidateLifetime = true
            }, out _);

            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            return userId?.Value;
        }
        catch
        {
            return null;
        }
    }

    public async Task<GetAuthDto?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
                return null;

            var user = await _refreshTokenRepository.GetUserByIdAsync(stored.UserId);
            if (user == null) return null;

            // Revoke old token
            stored.IsRevoked = true;
            await _refreshTokenRepository.UpdateRefreshTokenAsync(stored);

            // Get user roles from UserManager
            var roles = await _userManager.GetRolesAsync(user);
            var rolesList = roles.ToList();
            
            // Default to "User" if no roles assigned
            if (!rolesList.Any())
            {
                rolesList.Add("User");
            }

            // Generate new access + refresh pair with user's roles
            return await GenerateToken(user.Id, user.UserName!, rolesList);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        return await _refreshTokenRepository.RevokeTokenAsync(refreshToken);
    }

    private string GenerateRefreshToken()
    {
        var refreshKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Refresh_Key"]!));
        var creds = new SigningCredentials(refreshKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            expires: DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
