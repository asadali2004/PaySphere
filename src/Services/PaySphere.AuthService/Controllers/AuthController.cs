using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaySphere.AuthService.DTOs.Requests;
using PaySphere.AuthService.DTOs.Responses;
using PaySphere.AuthService.Services.Interfaces;
using PaySphere.BuildingBlocks.Responses;
using PaySphere.BuildingBlocks.Constants;
using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.AuthService.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _authService.RegisterAsync(request);

            var response = new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "User registered successfully.",
                Data = user,
                Errors = new List<string>()
            };

            return Created(string.Empty, response);
        }
        catch (BaseException ex) when (ex.Message == ErrorMessages.DuplicateUser)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new List<string> { ex.Message }
            };

            return Conflict(response);
        }
        catch (BaseException ex)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new List<string> { ex.Message }
            };

            return BadRequest(response);
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var login = await _authService.LoginAsync(request);

            var response = new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login successful.",
                Data = login,
                Errors = new List<string>()
            };

            return Ok(response);
        }
        catch (BaseException ex) when (ex.Message == ErrorMessages.InvalidCredentials)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new List<string> { ex.Message }
            };

            return Unauthorized(response);
        }
        catch (BaseException ex)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new List<string> { ex.Message }
            };

            return BadRequest(response);
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                var responseErr = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid token.",
                    Data = null,
                    Errors = new List<string> { "User id claim missing or invalid." }
                };

                return Unauthorized(responseErr);
            }

            var user = await _authService.GetProfileAsync(userId);

            var response = new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "User profile retrieved successfully.",
                Data = user,
                Errors = new List<string>()
            };

            return Ok(response);
        }
        catch (BaseException ex) when (ex.Message == ErrorMessages.UserNotFound)
        {
            var responseErr = new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new List<string> { ex.Message }
            };

            return Unauthorized(responseErr);
        }
        catch (BaseException ex)
        {
            var responseErr = new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new List<string> { ex.Message }
            };

            return BadRequest(responseErr);
        }
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                var responseErr = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid token.",
                    Data = null,
                    Errors = new List<string> { "User id claim missing or invalid." }
                };

                return Unauthorized(responseErr);
            }

            await _authService.ChangePasswordAsync(userId, request);

            return NoContent();
        }
        catch (BaseException ex) when (ex.Message == "Current password is incorrect.")
        {
            var responseErr = new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new List<string> { ex.Message }
            };

            return Unauthorized(responseErr);
        }
        catch (BaseException ex)
        {
            var responseErr = new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = new List<string> { ex.Message }
            };

            return BadRequest(responseErr);
        }
    }
}
