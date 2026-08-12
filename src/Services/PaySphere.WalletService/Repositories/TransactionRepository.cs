using Microsoft.EntityFrameworkCore;
using PaySphere.WalletService.Data;
using PaySphere.WalletService.Entities;
using PaySphere.WalletService.Repositories.Interfaces;

namespace PaySphere.WalletService.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly WalletDbContext _context;

    public TransactionRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }

    public async Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId)
    {
        return await _context.Transactions
            .Where(x => x.WalletId == walletId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByReferenceAsync(string reference)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(x => x.Reference == reference);
    }
}
