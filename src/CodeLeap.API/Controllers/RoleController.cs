using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Role;
using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeLeap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Create a new role (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var result = await _roleService.CreateRoleAsync(roleName);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _roleService.GetAllRolesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Assign roles to a user (Admin only)
        /// </summary>
        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto assignRolesDto)
        {
            var result = await _roleService.AssignRolesToUserAsync(assignRolesDto.UserId, assignRolesDto.Roles);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Remove roles from a user (Admin only)
        /// </summary>
        [HttpDelete("remove")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRoles([FromBody] AssignRolesDto assignRolesDto)
        {
            var result = await _roleService.RemoveRolesFromUserAsync(assignRolesDto.UserId, assignRolesDto.Roles);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get roles for a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var result = await _roleService.GetUserRolesAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete a role (Admin only)
        /// </summary>
        [HttpDelete("{roleName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var result = await _roleService.DeleteRoleAsync(roleName);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
