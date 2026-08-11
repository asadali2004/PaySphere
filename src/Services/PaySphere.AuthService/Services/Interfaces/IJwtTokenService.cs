using PaySphere.AuthService.Entities;

namespace PaySphere.AuthService.Services.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user, string roleName);
}
