using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Role;

namespace CodeLeap.Application.Interfaces
{
    public interface IRoleService
    {
        /// <summary>
        /// Create a new role
        /// </summary>
        Task<BaseResponseModel<bool>> CreateRoleAsync(string roleName);
        
        /// <summary>
        /// Delete an existing role
        /// </summary>
        Task<BaseResponseModel<bool>> DeleteRoleAsync(string roleName);
        
        /// <summary>
        /// Get all available roles
        /// </summary>
        Task<BaseResponseModel<IEnumerable<string>>> GetAllRolesAsync();
        
        /// <summary>
        /// Assign multiple roles to a user
        /// </summary>
        Task<BaseResponseModel<bool>> AssignRolesToUserAsync(string userId, List<string> roles);
        
        /// <summary>
        /// Remove specific roles from a user
        /// </summary>
        Task<BaseResponseModel<bool>> RemoveRolesFromUserAsync(string userId, List<string> roles);
        
        /// <summary>
        /// Get all roles assigned to a specific user
        /// </summary>
        Task<BaseResponseModel<UserRoleDto>> GetUserRolesAsync(string userId);
    }
}
