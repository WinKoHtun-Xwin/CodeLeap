using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeLeap.Application.DTOs.Auth;
using CodeLeap.Application.Common;
using Swashbuckle.AspNetCore.Annotations;

namespace CodeLeap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[SwaggerTag("Authentication endpoints for user registration, login, and token refresh")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "User Login",
        Description = "Authenticates a user with username and password, returns JWT access token and refresh token",
        OperationId = "Login"
    )]
    [SwaggerResponse(200, "Login successful", typeof(BaseResponseModel<GetAuthDto>))]
    [SwaggerResponse(400, "Invalid credentials", typeof(BaseResponseModel<GetAuthDto>))]
    public async Task<BaseResponseModel<GetAuthDto>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginUser(request);
        return result;
    }

    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "User Registration",
        Description = "Creates a new user account with the provided credentials",
        OperationId = "Register"
    )]
    [SwaggerResponse(200, "Registration successful", typeof(BaseResponseModel<bool>))]
    [SwaggerResponse(400, "Registration failed", typeof(BaseResponseModel<bool>))]
    public async Task<BaseResponseModel<bool>> Register([FromBody] RegisterNewUserDto registerNewUserDto)
    {
        var result = await _authService.RegisterNewUser(registerNewUserDto);
        return result;
    }

    [HttpPost("refreshToken/{refreshToken}")]
    [SwaggerOperation(
        Summary = "Refresh Token",
        Description = "Generates new JWT access token and refresh token using a valid refresh token",
        OperationId = "RefreshToken"
    )]
    [SwaggerResponse(200, "Token refresh successful", typeof(BaseResponseModel<GetAuthDto>))]
    [SwaggerResponse(400, "Invalid refresh token", typeof(BaseResponseModel<GetAuthDto>))]
    public async Task<BaseResponseModel<GetAuthDto>> Refresh(string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        return result;
    }
}
