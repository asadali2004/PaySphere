using PaySphere.WalletService.Entities;

namespace PaySphere.WalletService.Repositories.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(int id);

    Task<Wallet?> GetByUserIdAsync(int userId);

    Task<bool> ExistsByUserIdAsync(int userId);

    Task AddAsync(Wallet wallet);

    void Update(Wallet wallet);

    Task SaveChangesAsync();
}
