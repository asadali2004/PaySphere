using Microsoft.EntityFrameworkCore;
using PaySphere.AuthService.Entities;
using PaySphere.BuildingBlocks.Constants;

namespace PaySphere.AuthService.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaySphereAuthDbContext>();

        await dbContext.Database.MigrateAsync();

        if (await dbContext.Roles.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        dbContext.Roles.AddRange(
            new Role
            {
                Name = ApplicationConstants.AdminRole,
                Description = "Administrator role",
                CreatedAt = now
            },
            new Role
            {
                Name = ApplicationConstants.UserRole,
                Description = "Standard user role",
                CreatedAt = now
            });

        await dbContext.SaveChangesAsync();
    }
}