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

        private readonly ILoggerService<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IPasswordService passwordService, IJwtService jwtService, ILoggerService<AuthService> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<BaseResponseModel<GetAuthDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.Error("Refresh token is null or empty");
                    return BaseResponseModel<GetAuthDto>.Failure(
                        ResponseMessage.GeneralMessage.BadRequest
                    );
                }

                _logger.Info("Refreshing token");
                var getAuthDto = await _jwtService.RefreshTokenAsync(refreshToken);
                
                if (getAuthDto == null)
                {
                    _logger.Error("Failed to refresh token - invalid or expired token");
                    return BaseResponseModel<GetAuthDto>.Failure(
                        ResponseMessage.LoginMessage.InvalidCredentials
                    );
                }

                return BaseResponseModel<GetAuthDto>.SuccessResponse(
                    getAuthDto,
                    ResponseMessage.LoginMessage.Success
                );

            }
            catch (Exception ex)
            {
                _logger.Error("Error refreshing token", ex);
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
                _logger.Info("Logging in user By User : {username}", loginRequest.Username);
                var existingUser = await _userRepository.GetByUserNameAsync(loginRequest.Username);

                if (existingUser == null)
                {
                    _logger.Error("User not found By User : {username}", loginRequest.Username);
                    return BaseResponseModel<GetAuthDto>.Failure(
                        ResponseMessage.LoginMessage.UserNotFound
                    );
                }

                if (_passwordService.VerifyPassword(existingUser.Password, loginRequest.Password))
                {
                    var getAuthDto = await _jwtService.GenerateToken(existingUser.Id, existingUser.Username, "User");
                    
                    if (getAuthDto == null)
                    {
                        _logger.Error("Failed to generate token for user : {username}", loginRequest.Username);
                        return BaseResponseModel<GetAuthDto>.Failure(
                            ResponseMessage.LoginMessage.TokenGenerationFailed
                        );
                    }

                    _logger.Info("User logged in successfully By User : {username}", loginRequest.Username);
                    return BaseResponseModel<GetAuthDto>.SuccessResponse(
                        getAuthDto,
                        ResponseMessage.LoginMessage.Success
                    );
                }
                _logger.Error("Invalid credentials By User : {username}", loginRequest.Username);
                return BaseResponseModel<GetAuthDto>.Failure(
                    ResponseMessage.LoginMessage.InvalidCredentials
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error logging in user By User : {username}", loginRequest.Username, ex);
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
                _logger.Info("Registering new user By User : {username}", registerNewUserDto.Username);
                var existingUser = await _userRepository.GetByUserNameAsync(registerNewUserDto.Username.Trim());

                if (existingUser != null)
                {
                    _logger.Error("User already exists By User : {username}", registerNewUserDto.Username);
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
                    _logger.Error("User creation failed By User : {username}", registerNewUserDto.Username);
                    return BaseResponseModel<bool>.Failure(
                        ResponseMessage.RegisterNewUserMessage.RegistrationFailed
                    );
                }

                _logger.Info("User created successfully By User : {username}", registerNewUserDto.Username);

                return BaseResponseModel<bool>.SuccessResponse(
                    true,
                    ResponseMessage.RegisterNewUserMessage.Success
                );

            }
            catch (Exception ex)
            {
                _logger.Error("Error registering new user By User : {username}", registerNewUserDto.Username, ex);
                return BaseResponseModel<bool>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }

        }
    }
}
