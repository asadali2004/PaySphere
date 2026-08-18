using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaySphere.AuthService.DTOs.Requests;
using PaySphere.AuthService.DTOs.Responses;
using PaySphere.AuthService.Services.Interfaces;
using PaySphere.BuildingBlocks.Responses;
using PaySphere.BuildingBlocks.Constants;
using PaySphere.BuildingBlocks.Exceptions;

// Handles authentication HTTP endpoints (register, login, profile, change-password).
// Controllers are intentionally thin: they perform HTTP concerns and delegate
// business rules to services (IAuthService). This file contains only request
// routing and response adaptation.
namespace PaySphere.AuthService.Controllers;

[ApiController]
[Route("api/v1/auth")]
/// <summary>
/// Exposes authentication and user profile endpoints. Delegates core logic to
/// <see cref="PaySphere.AuthService.Services.Interfaces.IAuthService"/>.
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // Controllers are intentionally thin: they validate and adapt HTTP requests/responses
    // and delegate business rules to services. This keeps routing, model binding and HTTP
    // concerns separate from domain logic and makes the services easier to unit test.
    /// <summary>
    /// Registers a new user using the provided registration data. Returns the
    /// created user data in a standardized <see cref="PaySphere.BuildingBlocks.Responses.ApiResponse{T}"/>.
    /// Controllers map HTTP concerns and translate service exceptions into proper
    /// HTTP response codes.
    /// </summary>
    /// <param name="request">Registration details (email, password, role, etc.).</param>
    /// <returns>201 Created with the created user response on success; appropriate
    /// error responses otherwise.</returns>
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

    /// <summary>
    /// Authenticates a user and returns a JWT along with basic user information.
    /// On invalid credentials, returns 401 Unauthorized.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>200 OK with login token and user info on success; 401 on invalid credentials.</returns>
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

    /// <summary>
    /// Retrieves the authenticated user's profile. Requires a valid JWT with a
    /// NameIdentifier claim mapped to the integer user id.
    /// </summary>
    /// <returns>200 OK with user profile on success; 401 when the token or user is invalid.</returns>
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

    /// <summary>
    /// Changes the authenticated user's password. Current password is validated
    /// by the service. Returns 204 No Content on success.
    /// </summary>
    /// <param name="request">Contains current and new password details.</param>
    /// <returns>204 No Content on success; 401 if current password is incorrect.</returns>
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
