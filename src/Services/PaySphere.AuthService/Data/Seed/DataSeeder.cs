using Microsoft.EntityFrameworkCore;
using PaySphere.AuthService.Entities;
using PaySphere.AuthService.Helpers;
using PaySphere.BuildingBlocks.Constants;

namespace PaySphere.AuthService.Data.Seed;

/// <summary>
/// Seeds initial data required by the AuthService such as roles and an admin user.
/// This method is safe to run at startup in development and idempotent by design.
/// It performs EF Core migrations before seeding.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaySphereAuthDbContext>();

        await dbContext.Database.MigrateAsync();

        if (!await dbContext.Roles.AnyAsync())
        {
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

        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(x => x.Name == ApplicationConstants.AdminRole);
        if (adminRole is null)
        {
            return;
        }

        var adminEmail = "admin@paysphere.com";

        if (await dbContext.Users.AnyAsync(x => x.Email == adminEmail))
        {
            return;
        }

        dbContext.Users.Add(new User
        {
            FullName = "System Administrator",
            Email = adminEmail,
            PasswordHash = PasswordHasher.HashPassword("Admin@123"),
            PhoneNumber = "9999999999",
            RoleId = adminRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}
