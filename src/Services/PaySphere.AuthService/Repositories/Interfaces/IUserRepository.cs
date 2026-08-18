using PaySphere.AuthService.Entities;

namespace PaySphere.AuthService.Repositories.Interfaces;

/// <summary>
/// Repository abstraction for user-specific queries and persistence.
/// Keep database concerns here; do not implement business rules in repositories.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by email or null when not found.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Checks whether an email is already registered.
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Checks whether a phone number is already in use.
    /// </summary>
    Task<bool> PhoneExistsAsync(string phoneNumber);

    /// <summary>
    /// Returns the user including the associated role for claims issuance.
    /// </summary>
    Task<User?> GetUserWithRoleAsync(int userId);
}

