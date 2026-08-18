using PaySphere.AuthService.DTOs.Requests;
using PaySphere.AuthService.DTOs.Responses;

namespace PaySphere.AuthService.Services.Interfaces;

/// <summary>
/// Application service that encapsulates authentication business rules and
/// user-related operations exposed to controllers and other services.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user and returns the public user representation.
    /// </summary>
    Task<UserResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates credentials and returns a token response on success.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Retrieves a user's profile by id.
    /// </summary>
    Task<UserResponse> GetProfileAsync(int userId);

    /// <summary>
    /// Changes the password for the specified user after validating the current password.
    /// </summary>
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);

    /// <summary>
    /// Returns a minimal validation response intended for internal cross-service calls.
    /// </summary>
    Task<InternalUserValidationResponse> GetInternalUserValidationAsync(int userId);
}
