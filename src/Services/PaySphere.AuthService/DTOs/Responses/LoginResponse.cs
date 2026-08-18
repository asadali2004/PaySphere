using System;

namespace PaySphere.AuthService.DTOs.Responses;

/// <summary>
/// Response returned after a successful login containing the JWT token and expiry.
/// </summary>
public sealed class LoginResponse
{
    /// <summary>
    /// The JWT access token to be used in Authorization headers for downstream calls.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// UTC expiry time of the token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
