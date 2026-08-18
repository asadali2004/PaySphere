using PaySphere.BuildingBlocks.Base;

namespace PaySphere.AuthService.Entities;

/// <summary>
/// Represents a user role in the authentication database. Roles are simple
/// labels (e.g., Admin, User) used to issue role claims in JWT tokens.
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Name of the role (unique label).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description used for administration.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property containing users assigned to this role.
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}
