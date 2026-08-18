using Microsoft.EntityFrameworkCore;
using PaySphere.WalletService.Data;
using PaySphere.WalletService.Entities;
using PaySphere.WalletService.Repositories.Interfaces;

namespace PaySphere.WalletService.Repositories;

/// <summary>
/// Concrete repository implementing wallet persistence using EF Core.
/// Keep data access code here; business rules belong to services.
/// </summary>
public class WalletRepository : IWalletRepository
{
    private readonly WalletDbContext _context;

    public WalletRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByIdAsync(int id)
    {
        return await _context.Wallets.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Wallet?> GetByUserIdAsync(int userId)
    {
        return await _context.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<bool> ExistsByUserIdAsync(int userId)
    {
        return await _context.Wallets.AnyAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(Wallet wallet)
    {
        await _context.Wallets.AddAsync(wallet);
    }

    public void Update(Wallet wallet)
    {
        _context.Wallets.Update(wallet);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
