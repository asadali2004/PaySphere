using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaySphere.AuthService.DTOs.Responses;
using PaySphere.AuthService.Services.Interfaces;
using PaySphere.BuildingBlocks.Responses;

namespace PaySphere.AuthService.Controllers;

[ApiController]
[Route("api/v1/internal/users")]
public class InternalUsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public InternalUsersController(IAuthService authService)
    {
        _authService = authService;
    }

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
