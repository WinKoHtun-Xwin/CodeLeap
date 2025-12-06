using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Auth;
using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using Microsoft.AspNetCore.Identity;

namespace CodeLeap.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IJwtService _jwtService;
        private readonly ILoggerService<AuthService> _logger;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(
            UserManager<UserEntity> userManager,
            IJwtService jwtService,
            ILoggerService<AuthService> logger,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _logger = logger;
            _refreshTokenRepository = refreshTokenRepository;
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
                        ResponseMessage.LoginMessage.InvalidRefreshToken
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
                _logger.Info("Logging in user By User : {emailOrUsername}", loginRequest.EmailOrUsername);
                
                // Try to find user by username first, then by email
                var existingUser = await _userManager.FindByNameAsync(loginRequest.EmailOrUsername);
                
                if (existingUser == null)
                {
                    // If not found by username, try email
                    existingUser = await _userManager.FindByEmailAsync(loginRequest.EmailOrUsername);
                }

                if (existingUser == null)
                {
                    _logger.Error("User not found By User : {emailOrUsername}", loginRequest.EmailOrUsername);
                    return BaseResponseModel<GetAuthDto>.Failure(
                        ResponseMessage.LoginMessage.UserNotFound
                    );
                }

                // Verify password
                var isValid = await _userManager.CheckPasswordAsync(existingUser, loginRequest.Password);
                
                if (isValid)
                {
                    // ✅ SINGLE SESSION ENFORCEMENT: Revoke all existing sessions
                    await _refreshTokenRepository.RevokeAllUserTokensAsync(existingUser.Id);
                    
                    // Get user roles
                    var roles = await _userManager.GetRolesAsync(existingUser);
                    var rolesList = roles.ToList();
                    
                    // Default to "User" if no roles assigned
                    if (!rolesList.Any())
                    {
                        rolesList.Add("User");
                    }

                    // Generate new token (only this session will be active)
                    var getAuthDto = await _jwtService.GenerateToken(existingUser.Id, existingUser.UserName!, rolesList);
                    
                    if (getAuthDto == null)
                    {
                        _logger.Error("Failed to generate token for user By User : {emailOrUsername}", loginRequest.EmailOrUsername);
                        return BaseResponseModel<GetAuthDto>.Failure(
                            ResponseMessage.LoginMessage.TokenGenerationFailed
                        );
                    }

                    _logger.Info("User logged in successfully By User : {emailOrUsername}", loginRequest.EmailOrUsername);
                    return BaseResponseModel<GetAuthDto>.SuccessResponse(
                        getAuthDto,
                        ResponseMessage.LoginMessage.Success
                    );
                }
                
                _logger.Error("Invalid credentials By User : {emailOrUsername}", loginRequest.EmailOrUsername);
                return BaseResponseModel<GetAuthDto>.Failure(
                    ResponseMessage.LoginMessage.InvalidCredentials
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Login failed for user: {emailOrUsername}. Error: {error}", loginRequest.EmailOrUsername, ex.Message);
                return BaseResponseModel<GetAuthDto>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<bool>> RegisterNewUser(RegisterNewUserDto registerNewUserDto)
        {
            try
            {
                _logger.Info("Registering new user By User : {username}", registerNewUserDto.Username);
                
                // Check if user already exists by username
                var existingUser = await _userManager.FindByNameAsync(registerNewUserDto.Username.Trim());

                if (existingUser != null)
                {
                    _logger.Error("User already exists By User : {username}", registerNewUserDto.Username);
                    return BaseResponseModel<bool>.Failure(
                        ResponseMessage.RegisterNewUserMessage.UserAlreadyExists
                    );
                }
                
                // Check if email already exists
                var existingEmail = await _userManager.FindByEmailAsync(registerNewUserDto.Email.Trim());
                if (existingEmail != null)
                {
                    _logger.Error("Email already exists: {email}", registerNewUserDto.Email);
                    return BaseResponseModel<bool>.Failure(
                        "Email address is already registered"
                    );
                }

                // Create new user entity
                var userId = Guid.NewGuid().ToString();
                var newUser = new UserEntity
                {
                    Id = userId,
                    UserName = registerNewUserDto.Username.Trim(),
                    Email = registerNewUserDto.Email.Trim(),  // ✅ Save email
                    EmailConfirmed = false,  // Optional: require email confirmation
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsActive = true,
                    IsDeleted = false
                };

                // Use UserManager to create user with password (handles hashing automatically)
                var result = await _userManager.CreateAsync(newUser, registerNewUserDto.Password.Trim());

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.Error("User creation failed By User : {username}. Errors: {errors}", 
                        registerNewUserDto.Username, errors);
                    return BaseResponseModel<bool>.Failure(
                        ResponseMessage.RegisterNewUserMessage.RegistrationFailed,
                        errors
                    );
                }

                // Assign roles (default to "User" if not specified)
                var rolesToAssign = registerNewUserDto.Roles?.Any() == true 
                    ? registerNewUserDto.Roles 
                    : new List<string> { "User" };

                foreach (var role in rolesToAssign)
                {
                    // Check if role exists
                    var roleExists = await _userManager.GetUsersInRoleAsync(role);
                    if (roleExists == null)
                    {
                        _logger.Error("Role does not exist: {role}", role);
                        // Continue assigning other roles instead of failing the entire registration
                        continue;
                    }

                    var roleResult = await _userManager.AddToRoleAsync(newUser, role);
                    if (!roleResult.Succeeded)
                    {
                        _logger.Error("Failed to assign role {role} to user {username}", role, registerNewUserDto.Username);
                    }
                }

                _logger.Info("User created successfully By User : {username}",  registerNewUserDto.Username);

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
