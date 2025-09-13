using Microsoft.AspNetCore.Mvc;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Common;

namespace CodeLeap.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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
                    new { id = result.Data.Id }, 
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
