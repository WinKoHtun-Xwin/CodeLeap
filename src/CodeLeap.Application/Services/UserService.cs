using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Auth;

namespace CodeLeap.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService passwordService;
        private readonly IJwtService jwtService;

        public UserService(IUserRepository userRepository, IPasswordService passwordService, IJwtService jwtService)
        {
            _userRepository = userRepository;
            this.passwordService = passwordService;
            this.jwtService = jwtService;   
        }

        public async Task<BaseResponseModel<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                var userDtos = users.Select(MapToUserDto);
                
                return BaseResponseModel<IEnumerable<UserDto>>.SuccessResponse(
                    userDtos, 
                    ResponseMessage.UserMessage.GetAllSuccess
                );
            }
            catch (Exception ex)
            {
                return BaseResponseModel<IEnumerable<UserDto>>.Failure(
                    ResponseMessage.UserMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<UserDto>> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                
                if (user == null)
                {
                    return BaseResponseModel<UserDto>.Failure(
                        ResponseMessage.UserMessage.NotFound
                    );
                }

                return BaseResponseModel<UserDto>.SuccessResponse(
                    MapToUserDto(user),
                    ResponseMessage.UserMessage.GetSuccess
                );
            }
            catch (Exception ex)
            {
                return BaseResponseModel<UserDto>.Failure(
                    ResponseMessage.UserMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                createUserDto.Password = passwordService.HashPassword(createUserDto.Password);
                var userEntity = new UserEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = createUserDto.Username,
                    Password = createUserDto.Password, // Note: In production, hash the password
                    CreatedBy = "",
                    UpdatedBy = "",
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateUserAsync(userEntity);
                
                return BaseResponseModel<UserDto>.SuccessResponse(
                    MapToUserDto(createdUser),
                    ResponseMessage.UserMessage.CreatedSuccess
                );
            }
            catch (Exception ex)
            {
                return BaseResponseModel<UserDto>.Failure(
                    ResponseMessage.UserMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<UserDto>> UpdateUserAsync(string userId, CreateUserDto updateUserDto)
        {
            try
            {
                var userEntity = new UserEntity
                {
                    Id = userId,
                    Username = updateUserDto.Username,
                    Password = updateUserDto.Password, // Note: In production, hash the password
                    CreatedBy = "",
                    UpdatedBy = "",
                    UpdatedAt = DateTime.UtcNow
                };

                var updatedUser = await _userRepository.UpdateUserAsync(userId, userEntity);
                
                return BaseResponseModel<UserDto>.SuccessResponse(
                    MapToUserDto(updatedUser),
                    ResponseMessage.UserMessage.UpdatedSuccess
                );
            }
            catch (Exception ex)
            {
                return BaseResponseModel<UserDto>.Failure(
                    ResponseMessage.UserMessage.UpdatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<bool>> DeleteUserAsync(string id)
        {
            try
            {
                var result = await _userRepository.DeleteUserAsync(id);
                
                if (result)
                {
                    return BaseResponseModel<bool>.SuccessResponse(
                        true,
                        ResponseMessage.UserMessage.DeletedSuccess
                    );
                }
                else
                {
                    return BaseResponseModel<bool>.Failure(
                        ResponseMessage.UserMessage.DeletedFail
                    );
                }
            }
            catch (Exception ex)
            {
                return BaseResponseModel<bool>.Failure(
                    ResponseMessage.UserMessage.DeletedFail,
                    ex.Message
                );
            }
        }

        private static UserDto MapToUserDto(UserEntity userEntity)
        {
            return new UserDto
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                CreatedBy = userEntity.CreatedBy,
                UpdatedBy = userEntity.UpdatedBy,
                CreatedAt = userEntity.CreatedAt,
                UpdatedAt = userEntity.UpdatedAt
            };
        }
    }
}
