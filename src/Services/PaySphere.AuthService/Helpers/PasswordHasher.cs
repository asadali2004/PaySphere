using Microsoft.AspNetCore.Identity;
using PaySphere.AuthService.Entities;

namespace PaySphere.AuthService.Helpers;

/// <summary>
/// Thin wrapper around ASP.NET Core's PasswordHasher to centralize password hashing.
/// Using Identity's proven hashing ensures secure, salted hashes and simplifies verification.
/// The helper intentionally accepts plain strings and returns/compares hashes only; raw passwords are never stored.
/// </summary>
public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        var hasher = new PasswordHasher<User>();
        return hasher.HashPassword(new User(), password);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(new User(), hash, password);

        return result == PasswordVerificationResult.Success;
    }
}
