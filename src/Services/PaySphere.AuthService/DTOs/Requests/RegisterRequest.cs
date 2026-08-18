namespace PaySphere.AuthService.DTOs.Requests;

/// <summary>
/// DTO representing the registration data required to create a new user account.
/// This is an API contract and should not expose internal entity fields like password hashes.
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// The user's full display name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address used for login and communication.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional phone number for contact and verification.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Plain-text password supplied during registration. The service will hash
    /// this value before persisting; the plain text password must never be returned
    /// by any API response.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
