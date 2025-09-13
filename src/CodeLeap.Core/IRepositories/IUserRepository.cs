using CodeLeap.Core.Entities;

namespace CodeLeap.Core.IRepositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserEntity>> GetAllUsersAsync();

        Task<UserEntity?> GetUserByIdAsync(string id);

        Task<UserEntity> CreateUserAsync(UserEntity user);

        Task<UserEntity> UpdateUserAsync(string userId, UserEntity user);

        Task<Boolean> DeleteUserAsync(string id);
    }
}
