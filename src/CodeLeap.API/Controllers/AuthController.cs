using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeLeap.Application.DTOs.Auth;
using CodeLeap.Application.Common;

namespace CodeLeap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<BaseResponseModel<GetAuthDto>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginUser(request);
        return result;
    }

    [HttpPost("register")]
    public async Task<BaseResponseModel<bool>> Register([FromBody] RegisterNewUserDto registerNewUserDto)
    {
        var result = await _authService.RegisterNewUser(registerNewUserDto);
        return result;
    }

    [HttpPost("refreshToken/{refreshToken}")]
    public async Task<BaseResponseModel<GetAuthDto>> Refresh(string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        return result;
    }
}
