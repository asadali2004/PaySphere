using PaySphere.WalletService.Entities;

namespace PaySphere.WalletService.Repositories.Interfaces;

public interface ITransactionRepository
{
    /// <summary>
    /// Adds a transaction ledger entry.
    /// </summary>
    Task AddAsync(Transaction transaction);

    /// <summary>
    /// Returns all transactions for a wallet.
    /// </summary>
    Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId);

    /// <summary>
    /// Finds a transaction by its shared reference (useful for paired transfer entries).
    /// </summary>
    Task<Transaction?> GetByReferenceAsync(string reference);

    /// <summary>
    /// LINQ-based paginated query supporting search, filter and sorting.
    /// </summary>
    Task<(IEnumerable<Transaction> Transactions, int TotalRecords)> GetByWalletIdWithFiltersAsync(
        int walletId,
        string? search,
        int? type,
        DateTime? dateFrom,
        DateTime? dateTo,
        string sortBy,
        string sortOrder,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// Alternative stored-procedure based query returning the same shape as the LINQ query.
    /// Implemented as a demonstration of repository flexibility.
    /// </summary>
    Task<(IEnumerable<Transaction> Transactions, int TotalRecords)> GetByWalletIdUsingStoredProcedureAsync(
        int walletId,
        string? search,
        int? type,
        DateTime? dateFrom,
        DateTime? dateTo,
        string sortBy,
        string sortOrder,
        int pageNumber,
        int pageSize);
}
