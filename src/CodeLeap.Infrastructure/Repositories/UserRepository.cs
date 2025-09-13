using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using Microsoft.EntityFrameworkCore;

namespace CodeLeap.Infrastructure.Repositories
{
    public class UserRepository(PostgresSQLDbContext dbContext) : IUserRepository
    {
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
            user.Id = Guid.NewGuid().ToString();
            var entityEntry = await dbContext.Set<UserEntity>().AddAsync(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<UserEntity> UpdateUserAsync(string userId, UserEntity user)
        {
            var existUser = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
            if (existUser == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            existUser.Username = user.Username;
            existUser.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return existUser;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var existUser = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id);
            if (existUser == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            dbContext.Set<UserEntity>().Remove(existUser);
            return await dbContext.SaveChangesAsync() > 0;
        }
    }
}
