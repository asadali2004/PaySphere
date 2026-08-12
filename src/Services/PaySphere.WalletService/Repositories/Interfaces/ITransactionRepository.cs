using PaySphere.WalletService.Entities;

namespace PaySphere.WalletService.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);

    Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId);

    Task<Transaction?> GetByReferenceAsync(string reference);
}
