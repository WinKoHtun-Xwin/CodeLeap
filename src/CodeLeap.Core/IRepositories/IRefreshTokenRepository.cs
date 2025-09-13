using CodeLeap.Core.Entities;

namespace CodeLeap.Core.IRepositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshTokenEntity> CreateRefreshTokenAsync(RefreshTokenEntity refreshToken);
        Task<RefreshTokenEntity?> GetByTokenAsync(string token);
        Task<RefreshTokenEntity> UpdateRefreshTokenAsync(RefreshTokenEntity refreshToken);
        Task<bool> RevokeTokenAsync(string token);
        Task<UserEntity?> GetUserByIdAsync(string userId);
    }
}
