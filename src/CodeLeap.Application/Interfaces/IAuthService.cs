using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Auth;

namespace CodeLeap.Application.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponseModel<GetAuthDto>> RefreshTokenAsync(string refreshToken);
        Task<BaseResponseModel<GetAuthDto>> LoginUser(LoginRequest loginRequest);
        Task<BaseResponseModel<bool>> RegisterNewUser(RegisterNewUserDto registerNewUserDto);
     }
}
