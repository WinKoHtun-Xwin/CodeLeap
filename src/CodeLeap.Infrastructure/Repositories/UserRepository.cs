using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using Microsoft.EntityFrameworkCore;

namespace CodeLeap.Infrastructure.Repositories
{
    public class UserRepository(PostgresSqlDbContext dbContext) : IUserRepository
    {

        public async Task<UserEntity?> GetByUserNameAsync(string username)
        {
            // Identity uses UserName property instead of Username
            return await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.UserName == username.Trim());
        }

        public async Task<IEnumerable<UserEntity>> GetAllUsersAsync()
        {
            return await dbContext.Set<UserEntity>().ToListAsync();
        }

        public async Task<UserEntity?> GetUserByIdAsync(string id)
        {
            return await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<UserEntity> CreateUserAsync(UserEntity user)
        {
            // NOTE: For Identity users, prefer using UserManager.CreateAsync instead
            // This method is kept for compatibility
            user.Id = Guid.NewGuid().ToString();
            await dbContext.Set<UserEntity>().AddAsync(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<UserEntity> UpdateUserAsync(string userId, UserEntity user)
        {
            var existUser = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new KeyNotFoundException("User not found");

            // Update Identity UserName property
            existUser.UserName = user.UserName;
            existUser.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return existUser;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var existUser = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("User not found");

            dbContext.Set<UserEntity>().Remove(existUser);
            return await dbContext.SaveChangesAsync() > 0;
        }
    }
}
