using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Role;
using CodeLeap.Application.Interfaces;
using CodeLeap.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace CodeLeap.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly ILoggerService<RoleService> _logger;

        public RoleService(
            RoleManager<IdentityRole> roleManager,
            UserManager<UserEntity> userManager,
            ILoggerService<RoleService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<BaseResponseModel<bool>> CreateRoleAsync(string roleName)
        {
            try
            {
                _logger.Info("Creating role: {roleName}", roleName);

                if (string.IsNullOrWhiteSpace(roleName))
                {
                    return BaseResponseModel<bool>.Failure("Role name cannot be empty");
                }

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (roleExists)
                {
                    _logger.Error("Role already exists: {roleName}", roleName);
                    return BaseResponseModel<bool>.Failure($"Role '{roleName}' already exists");
                }

                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.Error("Failed to create role: {roleName}. Errors: {errors}", roleName, errors);
                    return BaseResponseModel<bool>.Failure("Failed to create role", errors);
                }

                _logger.Info("Role created successfully: {roleName}", roleName);
                return BaseResponseModel<bool>.SuccessResponse(true, $"Role '{roleName}' created successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Error creating role: {roleName}", roleName, ex);
                return BaseResponseModel<bool>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<bool>> DeleteRoleAsync(string roleName)
        {
            try
            {
                _logger.Info("Deleting role: {roleName}", roleName);

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.Error("Role not found: {roleName}", roleName);
                    return BaseResponseModel<bool>.Failure($"Role '{roleName}' not found");
                }

                var result = await _roleManager.DeleteAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.Error("Failed to delete role: {role Name}. Errors: {errors}", roleName, errors);
                    return BaseResponseModel<bool>.Failure("Failed to delete role", errors);
                }

                _logger.Info("Role deleted successfully: {roleName}", roleName);
                return BaseResponseModel<bool>.SuccessResponse(true, $"Role '{roleName}' deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting role: {roleName}", roleName, ex);
                return BaseResponseModel<bool>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }
        }

        public Task<BaseResponseModel<IEnumerable<string>>> GetAllRolesAsync()
        {
            try
            {
                _logger.Info("Getting all roles");

                var roles = _roleManager.Roles.Select(r => r.Name).Where(n => n != null).Cast<string>().ToList();

                _logger.Info("Retrieved {count} roles", roles.Count);
                return Task.FromResult(BaseResponseModel<IEnumerable<string>>.SuccessResponse(roles, "Roles retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting all roles", ex);
                return Task.FromResult(BaseResponseModel<IEnumerable<string>>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                ));
            }
        }

        public async Task<BaseResponseModel<bool>> AssignRolesToUserAsync(string userId, List<string> roles)
        {
            try
            {
                _logger.Info("Assigning roles to user: {userId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Error("User not found: {userId}", userId);
                    return BaseResponseModel<bool>.Failure("User not found");
                }

                // Validate all roles exist
                foreach (var roleName in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        _logger.Error("Role does not exist: {roleName}", roleName);
                        return BaseResponseModel<bool>.Failure($"Role '{roleName}' does not exist");
                    }
                }

                // Remove existing roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.Error("Failed to remove existing roles from user: {userId}. Errors: {errors}", userId, errors);
                        return BaseResponseModel<bool>.Failure("Failed to remove existing roles", errors);
                    }
                }

                // Add new roles
                var addResult = await _userManager.AddToRolesAsync(user, roles);

                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    _logger.Error("Failed to assign roles to user: {userId}. Errors: {errors}", userId, errors);
                    return BaseResponseModel<bool>.Failure("Failed to assign roles", errors);
                }

                _logger.Info("Roles assigned successfully to user: {userId}", userId);
                return BaseResponseModel<bool>.SuccessResponse(true, "Roles assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Error assigning roles to user: {userId}", userId, ex);
                return BaseResponseModel<bool>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<bool>> RemoveRolesFromUserAsync(string userId, List<string> roles)
        {
            try
            {
                _logger.Info("Removing roles from user: {userId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Error("User not found: {userId}", userId);
                    return BaseResponseModel<bool>.Failure("User not found");
                }

                var result = await _userManager.RemoveFromRolesAsync(user, roles);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.Error("Failed to remove roles from user: {userId}. Errors: {errors}", userId, errors);
                    return BaseResponseModel<bool>.Failure("Failed to remove roles", errors);
                }

                _logger.Info("Roles removed successfully from user: {userId}", userId);
                return BaseResponseModel<bool>.SuccessResponse(true, "Roles removed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Error removing roles from user: {userId}", userId, ex);
                return BaseResponseModel<bool>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<UserRoleDto>> GetUserRolesAsync(string userId)
        {
            try
            {
                _logger.Info("Getting roles for user: {userId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Error("User not found: {userId}", userId);
                    return BaseResponseModel<UserRoleDto>.Failure("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userRoleDto = new UserRoleDto
                {
                    UserId = userId,
                    Roles = roles.ToList()
                };

                _logger.Info("Retrieved {count} roles for user: {userId}", roles.Count, userId);
                return BaseResponseModel<UserRoleDto>.SuccessResponse(userRoleDto, "User roles retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting user roles: {userId}", userId, ex);
                return BaseResponseModel<UserRoleDto>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    ex.Message
                );
            }
        }
    }
}
