namespace PaySphere.AuthService.DTOs.Requests;

/// <summary>
/// DTO for user login credentials. Sent to the authentication endpoint to
/// obtain a JWT token when valid.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// The user's email used as the credential identifier.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's plain-text password. The API will validate and not return this value.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
