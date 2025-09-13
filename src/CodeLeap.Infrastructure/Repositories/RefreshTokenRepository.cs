using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using Microsoft.EntityFrameworkCore;

namespace CodeLeap.Infrastructure.Repositories
{
    public class RefreshTokenRepository(PostgresSQLDbContext dbContext) : IRefreshTokenRepository
    {
        public async Task<RefreshTokenEntity> CreateRefreshTokenAsync(RefreshTokenEntity refreshToken)
        {
            refreshToken.Id = Guid.NewGuid().ToString();
            await dbContext.Set<RefreshTokenEntity>().AddAsync(refreshToken);
            await dbContext.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<RefreshTokenEntity?> GetByTokenAsync(string token)
        {
            return await dbContext.Set<RefreshTokenEntity>()
                .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked);
        }

        public async Task<RefreshTokenEntity> UpdateRefreshTokenAsync(RefreshTokenEntity refreshToken)
        {
            dbContext.Set<RefreshTokenEntity>().Update(refreshToken);
            await dbContext.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var storedToken = await dbContext.Set<RefreshTokenEntity>()
                .FirstOrDefaultAsync(r => r.Token == token);
            
            if (storedToken == null) return false;

            storedToken.IsRevoked = true;
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<UserEntity?> GetUserByIdAsync(string userId)
        {
            return await dbContext.Set<UserEntity>().FindAsync(userId);
        }
    }
}
