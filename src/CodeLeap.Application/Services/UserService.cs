using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using CodeLeap.Application.Common;

namespace CodeLeap.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ILoggerService<UserService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        
        public UserService(
            IUserRepository userRepository, 
            ICurrentUserService currentUserService, 
            ILoggerService<UserService> logger)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<BaseResponseModel<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                _logger.Info("Getting all users By User : {userId}", _currentUserService.UserId!);
                var users = await _userRepository.GetAllUsersAsync();
                var userDtos = users.Select(MapToUserDto).Where(dto => dto != null).Cast<UserDto>();
                
                return BaseResponseModel<IEnumerable<UserDto>>.SuccessResponse(
                    userDtos, 
                    ResponseMessage.UserMessage.GetAllSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting all users By User : {userId}", _currentUserService.UserId!, ex);
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
                _logger.Info("Getting user by id : {id} By User : {userId}", id, _currentUserService.UserId!);
                var user = await _userRepository.GetUserByIdAsync(id);
                
                if (user == null)
                {
                    _logger.Error("User not found : {id} By User : {userId}", id, _currentUserService.UserId!);
                    return BaseResponseModel<UserDto>.Failure(
                        ResponseMessage.UserMessage.NotFound
                    );
                }

                _logger.Info("User found : {id} By User : {userId}", id, _currentUserService.UserId!);

                return BaseResponseModel<UserDto>.SuccessResponse(
                    MapToUserDto(user),
                    ResponseMessage.UserMessage.GetSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting user by id By User : {userId}", _currentUserService.UserId!, ex);
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
                _logger.Info("Creating user By User : {userId}", _currentUserService.UserId!);
                
                // WARNING: For Identity users, this should use UserManager.CreateAsync instead
                // This method bypasses Identity's password hashing and validation
                var userEntity = new UserEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = createUserDto.Username,
                    // Password should be set via UserManager.CreateAsync, not directly
                    CreatedBy = _currentUserService.UserId!,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                var createdUser = await _userRepository.CreateUserAsync(userEntity);
                
                if (createdUser == null)
                {
                    _logger.Error("User creation failed By User : {userId}", _currentUserService.UserId!);
                    return BaseResponseModel<UserDto>.Failure(
                        ResponseMessage.UserMessage.CreatedFail
                    );
                }

                _logger.Info("User created successfully By User : {userId}", _currentUserService.UserId!);

                return BaseResponseModel<UserDto>.SuccessResponse(
                    MapToUserDto(createdUser)!,
                    ResponseMessage.UserMessage.CreatedSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error creating user By User : {userId}", _currentUserService.UserId!, ex);
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
                _logger.Info("Updating user By User : {userId}", _currentUserService.UserId!);
                
                // WARNING: For Identity users, password changes should use UserManager.ChangePasswordAsync
                var userEntity = new UserEntity
                {
                    Id = userId,
                    UserName = updateUserDto.Username,
                    // Password updates should be done via UserManager, not directly
                    CreatedBy = _currentUserService.UserId!,
                    UpdatedAt = DateTime.UtcNow
                };

                var updatedUser = await _userRepository.UpdateUserAsync(userId, userEntity);
                
                if (updatedUser == null)
                {
                    _logger.Error("User update failed - user not found : {userId} By User : {currentUserId}", userId, _currentUserService.UserId!);
                    return BaseResponseModel<UserDto>.Failure(
                        ResponseMessage.UserMessage.NotFound
                    );
                }

                _logger.Info("User updated successfully By User : {userId}", _currentUserService.UserId!);

                return BaseResponseModel<UserDto>.SuccessResponse(
                    MapToUserDto(updatedUser)!,
                    ResponseMessage.UserMessage.UpdatedSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating user By User : {userId}", _currentUserService.UserId!, ex);
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
                _logger.Info("Deleting user");
                var result = await _userRepository.DeleteUserAsync(id);
                
                if (result)
                {
                    _logger.Info("User deleted successfully");

                    return BaseResponseModel<bool>.SuccessResponse(
                        true,
                        ResponseMessage.UserMessage.DeletedSuccess
                    );
                }
                else
                {
                    _logger.Error("User not deleted");

                    return BaseResponseModel<bool>.Failure(
                        ResponseMessage.UserMessage.DeletedFail
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting user", ex);
                return BaseResponseModel<bool>.Failure(
                    ResponseMessage.UserMessage.DeletedFail,
                    ex.Message
                );
            }
        }

        private static UserDto? MapToUserDto(UserEntity? userEntity)
        {
            if (userEntity == null)
                return null;

            return new UserDto
            {
                Id = userEntity.Id,
                Username = userEntity.UserName ?? string.Empty,
                CreatedBy = userEntity.CreatedBy,
                UpdatedBy = userEntity.UpdatedBy,
                CreatedAt = userEntity.CreatedAt,
                UpdatedAt = userEntity.UpdatedAt
            };
        }
    }
}
