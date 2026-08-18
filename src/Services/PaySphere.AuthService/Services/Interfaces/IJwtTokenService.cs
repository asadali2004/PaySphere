using PaySphere.AuthService.Entities;

namespace PaySphere.AuthService.Services.Interfaces;

/// <summary>
/// Responsible for generating JWT tokens for authenticated users.
/// Implementations encapsulate signing, claim construction, and expiry policy.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token and its expiry for the specified user and role.
    /// </summary>
    /// <param name="user">User entity used to populate identity claims.</param>
    /// <param name="roleName">Role label included in role claims.</param>
    /// <returns>A tuple containing the token string and its UTC expiry time.</returns>
    (string Token, DateTime ExpiresAt) GenerateToken(User user, string roleName);
}
