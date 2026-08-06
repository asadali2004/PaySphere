using PaySphere.AuthService.Entities;

namespace PaySphere.AuthService.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<bool> EmailExistsAsync(string email);

    Task<User?> GetUserWithRoleAsync(int userId);
}
