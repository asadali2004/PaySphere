using Microsoft.EntityFrameworkCore;
using PaySphere.AuthService.Entities;

namespace PaySphere.AuthService.Data;

/// <summary>
/// EF Core DbContext for the AuthService containing Users and Roles sets.
/// Model configurations are applied from the assembly to centralize mapping logic.
/// </summary>
public class PaySphereAuthDbContext : DbContext
{
    public PaySphereAuthDbContext(DbContextOptions<PaySphereAuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaySphereAuthDbContext).Assembly);
    }
}
