using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Common;
using CodeLeap.API.Extensions;

namespace CodeLeap.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protect all endpoints in this controller
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public UserController(IUserService userService, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseModel<IEnumerable<UserDto>>>> GetAllUsers()
        {
            var result = await _userService.GetAllUsersAsync();
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        /// <summary>
        /// Get current user profile - demonstrates different ways to get UserID from token
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<BaseResponseModel<UserDto>>> GetCurrentUser()
        {
            // Method 1: Using the CurrentUserService (Dependency Injection)
            var userId1 = _currentUserService.GetUserId();
            
            // Method 2: Using the BaseController methods
            var userId2 = GetCurrentUserId();
            
            // Method 3: Using extension methods
            var userId3 = User.GetUserId();
            
            // Method 4: Direct access to claims
            var userId4 = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            // All methods should return the same userId
            var currentUserId = userId1 ?? userId2 ?? userId3 ?? userId4;
            
            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest(BaseResponseModel<UserDto>.Failure(
                    "Unable to get user ID from token", 
                    "Authentication error"
                ));
            }

            var result = await _userService.GetUserByIdAsync(currentUserId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }

        /// <summary>
        /// Get current user info - demonstrates getting all user claims
        /// </summary>
        [HttpGet("me/info")]
        public ActionResult<object> GetCurrentUserInfo()
        {
            var userInfo = new
            {
                UserId = User.GetUserId(),
                Username = User.GetUsername(),
                Role = User.GetUserRole(),
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                
                // Alternative methods:
                UserIdFromService = _currentUserService.GetUserId(),
                UsernameFromService = _currentUserService.GetUsername(),
                RoleFromService = _currentUserService.GetUserRole(),
                
                // All claims for debugging
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            return Ok(userInfo);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel<UserDto>>> GetUserById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(BaseResponseModel<UserDto>.Failure(
                    "User ID is required", 
                    "Invalid ID parameter"
                ));
            }

            var result = await _userService.GetUserByIdAsync(id);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseModel<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            // ModelState validation is now handled by global ModelValidationFilter
            var result = await _userService.CreateUserAsync(createUserDto);
            
            if (result.Success)
            {
                return CreatedAtAction(
                    nameof(GetUserById), 
                    new { id = result?.Data?.Id }, 
                    result
                );
            }
            
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel<UserDto>>> UpdateUser(string id, [FromBody] CreateUserDto updateUserDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(BaseResponseModel<UserDto>.Failure(
                    "User ID is required", 
                    "Invalid ID parameter"
                ));
            }

            // ModelState validation is now handled by global ModelValidationFilter
            var result = await _userService.UpdateUserAsync(id, updateUserDto);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseModel<bool>>> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(BaseResponseModel<bool>.Failure(
                    "User ID is required", 
                    "Invalid ID parameter"
                ));
            }

            var result = await _userService.DeleteUserAsync(id);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
    }
}
