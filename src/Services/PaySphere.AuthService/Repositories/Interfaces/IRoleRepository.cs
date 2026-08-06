using PaySphere.AuthService.Entities;

namespace PaySphere.AuthService.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);

    Task<Role?> GetByIdAsync(int id);
}
