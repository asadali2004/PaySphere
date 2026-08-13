using PaySphere.BuildingBlocks.Base;

namespace PaySphere.AuthService.Entities;

/// <summary>
/// Represents an application user stored in the AuthService database.
/// The entity stores the PasswordHash but never returns it to clients or other services.
/// Storing RoleId and IsActive here keeps authentication concerns isolated from other services.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Persisted password hash. Never returned to clients and only used for verification.
    public string PasswordHash { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    // Foreign key to Role configuration. Roles are simple and stored in the same DB for this service.
    public int RoleId { get; set; }

    // Simple active flag to allow disabling accounts without deleting rows.
    public bool IsActive { get; set; }

    // Navigation property - included so EF Core can eager-load role details where needed.
    public Role Role { get; set; } = null!;
}
