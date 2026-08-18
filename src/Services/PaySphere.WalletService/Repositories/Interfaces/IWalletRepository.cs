using PaySphere.WalletService.Entities;

namespace PaySphere.WalletService.Repositories.Interfaces;

/// <summary>
/// Repository abstraction for wallet persistence operations. Repositories
/// encapsulate EF Core access and do not contain business logic.
/// </summary>
public interface IWalletRepository
{
    /// <summary>
    /// Gets a wallet by its primary key.
    /// </summary>
    Task<Wallet?> GetByIdAsync(int id);

    /// <summary>
    /// Returns the wallet for a given user id.
    /// </summary>
    Task<Wallet?> GetByUserIdAsync(int userId);

    /// <summary>
    /// Returns true when a user already has a wallet.
    /// </summary>
    Task<bool> ExistsByUserIdAsync(int userId);

    /// <summary>
    /// Adds a new wallet entity.
    /// </summary>
    Task AddAsync(Wallet wallet);

    /// <summary>
    /// Marks the wallet as modified.
    /// </summary>
    void Update(Wallet wallet);

    /// <summary>
    /// Persists changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
