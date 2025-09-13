using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Auth;

namespace CodeLeap.Application.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponseModel<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<BaseResponseModel<UserDto>> GetUserByIdAsync(string id);
        Task<BaseResponseModel<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<BaseResponseModel<UserDto>> UpdateUserAsync(string userId, CreateUserDto updateUserDto);
        Task<BaseResponseModel<bool>> DeleteUserAsync(string id);
    }
}
