namespace PaySphere.AuthService.DTOs.Responses;

/// <summary>
/// Public user representation returned by authentication APIs. Does NOT include
/// sensitive data such as password hash.
/// </summary>
public sealed class UserResponse
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The user's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The user's role name (e.g., Admin, User).
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
