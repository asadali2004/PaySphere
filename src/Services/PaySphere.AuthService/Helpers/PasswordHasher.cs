using Microsoft.AspNetCore.Identity;
using PaySphere.AuthService.Entities;

namespace PaySphere.AuthService.Helpers;

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
