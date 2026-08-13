using PaySphere.WalletService.Entities;

namespace PaySphere.WalletService.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);

    Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId);

    Task<Transaction?> GetByReferenceAsync(string reference);

    // LINQ-based paged query with search/filter/sort
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

    // Stored procedure based query returning same shape
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
