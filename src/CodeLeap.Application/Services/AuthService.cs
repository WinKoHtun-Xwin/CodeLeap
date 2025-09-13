using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Auth;
using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeLeap.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IPasswordService passwordService, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public async Task<BaseResponseModel<GetAuthDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var getAuthDto = await _jwtService.RefreshTokenAsync(refreshToken);
                return BaseResponseModel<GetAuthDto>.SuccessResponse(
                    getAuthDto,
                    ResponseMessage.loginMessage.Success
                );

            }
            catch (Exception ex)
            {
                return BaseResponseModel<GetAuthDto>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<GetAuthDto>> LoginUser(LoginRequest loginRequest)
        {
            try
            {
                var existingUser = await _userRepository.GetByUserNameAsync(loginRequest.Username);

                if (existingUser == null)
                {
                    return BaseResponseModel<GetAuthDto>.Failure(
                        ResponseMessage.loginMessage.UserNotFound
                    );
                }

                if (_passwordService.VerifyPassword(existingUser.Password, loginRequest.Password))
                {
                    var getAuthDto = await _jwtService.GenerateToken(existingUser.Id, existingUser.Username, "User");
                    return BaseResponseModel<GetAuthDto>.SuccessResponse(
                        getAuthDto,
                        ResponseMessage.loginMessage.Success
                    );
                }
                return BaseResponseModel<GetAuthDto>.Failure(
                    ResponseMessage.loginMessage.InvalidCredentials
                );
            }
            catch (Exception ex)
            {
                return BaseResponseModel<GetAuthDto>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<bool>> RegisterNewUser (RegisterNewUserDto registerNewUserDto)
        {
            try
            {
                var existingUser = await _userRepository.GetByUserNameAsync(registerNewUserDto.Username.Trim());

                if (existingUser != null)
                {
                    return BaseResponseModel<bool>.Failure(
                        ResponseMessage.RegisterNewUserMessage.UserAlreadyExists
                    );
                }

                var hashedPassword = _passwordService.HashPassword(registerNewUserDto.Password.Trim());
                var UserID = Guid.NewGuid().ToString();
                UserEntity createUser = new UserEntity
                {
                    Id = UserID,
                    Username = registerNewUserDto.Username.Trim(),
                    Password = hashedPassword,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = UserID
                };

                var result = await _userRepository.CreateUserAsync(createUser);

                if (result == null)
                {
                    return BaseResponseModel<bool>.Failure(
                        ResponseMessage.RegisterNewUserMessage.RegistrationFailed
                    );
                }

                return BaseResponseModel<bool>.SuccessResponse(
                    true,
                    ResponseMessage.RegisterNewUserMessage.Success
                );

            }
            catch (Exception ex)
            {
                return BaseResponseModel<bool>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }

        }
    }
}
