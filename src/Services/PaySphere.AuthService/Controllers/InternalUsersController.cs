using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaySphere.AuthService.DTOs.Responses;
using PaySphere.AuthService.Services.Interfaces;
using PaySphere.BuildingBlocks.Responses;

// Internal users endpoint used by other services to validate user existence and basic info.
// This controller is typically called internally (by WalletService) and remains
// lightweight: it performs request routing and forwards validation to IAuthService.
namespace PaySphere.AuthService.Controllers;

[ApiController]
[Route("api/v1/internal/users")]
/// <summary>
/// Provides internal-only user validation endpoints used by other microservices.
/// Responses are shaped as <see cref="PaySphere.BuildingBlocks.Responses.ApiResponse{T}"/>.
/// </summary>
public class InternalUsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public InternalUsersController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Returns minimal user validation information for the given user id.
    /// Used by downstream services to verify receiver existence before operations.
    /// </summary>
    /// <param name="id">The user id to validate.</param>
    /// <returns>200 OK with validation response.</returns>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserValidation(int id)
    {
        var user = await _authService.GetInternalUserValidationAsync(id);

        var response = new ApiResponse<InternalUserValidationResponse>
        {
            Success = true,
            Message = "User validation retrieved successfully.",
            Data = user,
            Errors = new List<string>()
        };

        return Ok(response);
    }
}
