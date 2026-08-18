namespace PaySphere.AuthService.DTOs.Requests;

/// <summary>
/// DTO used when an authenticated user wants to change their password.
/// The service validates the current password and applies the new one.
/// </summary>
public sealed class ChangePasswordRequest
{
    /// <summary>
    /// The user's current password for validation.
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// The desired new password.
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}
