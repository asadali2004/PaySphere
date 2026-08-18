using Microsoft.EntityFrameworkCore;
using PaySphere.AuthService.Data;
using PaySphere.AuthService.Entities;
using PaySphere.AuthService.Repositories.Interfaces;

namespace PaySphere.AuthService.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    /// <summary>
    /// Concrete repository for Role entities. Inherits common behavior from GenericRepository
    /// and exposes role-specific queries used by authentication services.
    /// </summary>
    public RoleRepository(PaySphereAuthDbContext context)
        : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await Context.Roles
            .FirstOrDefaultAsync(x => x.Name == roleName);
    }
}
