using Microsoft.EntityFrameworkCore;
using PaySphere.WalletService.Entities;

namespace PaySphere.WalletService.Data;

/// <summary>
/// EF Core DbContext for the WalletService containing Wallet and Transaction sets.
/// Model configurations are applied from the assembly to keep entity mapping centralized.
/// </summary>
public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options)
        : base(options)
    {
    }

    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WalletDbContext).Assembly);
    }
}
