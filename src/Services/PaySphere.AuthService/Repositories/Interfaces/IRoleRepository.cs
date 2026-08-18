using PaySphere.AuthService.Entities;

namespace PaySphere.AuthService.Repositories.Interfaces;

/// <summary>
/// Repository abstraction for role-related queries.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Retrieves a role by its name label.
    /// </summary>
    Task<Role?> GetByNameAsync(string roleName);

    /// <summary>
    /// Retrieves a role by id.
    /// </summary>
    Task<Role?> GetByIdAsync(int id);
}
