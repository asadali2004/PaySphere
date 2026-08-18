namespace PaySphere.AuthService.DTOs.Responses;

/// <summary>
/// Minimal response used by other services to validate whether a user exists
/// and is active. Designed for internal cross-service communication.
/// </summary>
public class InternalUserValidationResponse
{
    /// <summary>
    /// The id of the user being validated.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// True when the user record exists.
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// True when the user's account is active and allowed to participate in workflows.
    /// </summary>
    public bool IsActive { get; set; }
}
