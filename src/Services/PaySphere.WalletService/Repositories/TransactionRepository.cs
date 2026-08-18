using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaySphere.WalletService.Data;
using PaySphere.WalletService.Entities;
using PaySphere.WalletService.Repositories.Interfaces;

namespace PaySphere.WalletService.Repositories;

public class TransactionRepository : ITransactionRepository
{
    /// <summary>
    /// Concrete repository implementing transaction queries and storage. Provides
    /// both LINQ-based queries and a stored-procedure alternative for retrieving
    /// paginated transaction history. Repository encapsulates DB access details.
    /// </summary>
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

    public async Task<(IEnumerable<Transaction> Transactions, int TotalRecords)> GetByWalletIdWithFiltersAsync(
        int walletId,
        string? search,
        int? type,
        DateTime? dateFrom,
        DateTime? dateTo,
        string sortBy,
        string sortOrder,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Transactions.AsQueryable()
            .Where(x => x.WalletId == walletId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x => (x.Reference != null && x.Reference.Contains(s)) || (x.Description != null && x.Description.Contains(s)));
        }

        if (type.HasValue)
        {
            query = query.Where(x => (int)x.Type == type.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= dateTo.Value);
        }

        var total = await query.CountAsync();

        var sort = (sortBy ?? "createdAt").Trim().ToLowerInvariant();
        var order = (sortOrder ?? "desc").Trim().ToLowerInvariant();

        if (sort == "amount")
        {
            // SQLite provider has limitations ordering by decimal server-side in some versions.
            // Materialize and order in memory when using Sqlite to keep tests stable.
            if ((_context.Database.ProviderName?.IndexOf("sqlite", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
            {
                var list = await query.ToListAsync();
                var ordered = order == "asc" ? list.OrderBy(x => x.Amount).ToList() : list.OrderByDescending(x => x.Amount).ToList();
                var itemsInPage = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return (itemsInPage, total);
            }

            query = order == "asc" ? query.OrderBy(x => x.Amount) : query.OrderByDescending(x => x.Amount);
        }
        else
        {
            query = order == "asc" ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Transaction> Transactions, int TotalRecords)> GetByWalletIdUsingStoredProcedureAsync(
        int walletId,
        string? search,
        int? type,
        DateTime? dateFrom,
        DateTime? dateTo,
        string sortBy,
        string sortOrder,
        int pageNumber,
        int pageSize)
    {
        var results = new List<Transaction>();
        int total = 0;

        var conn = _context.Database.GetDbConnection();
        try
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "GetTransactionsForWallet";
            cmd.CommandType = CommandType.StoredProcedure;

            var pWallet = cmd.CreateParameter(); pWallet.ParameterName = "@WalletId"; pWallet.Value = walletId; pWallet.DbType = DbType.Int32; cmd.Parameters.Add(pWallet);
            var pSearch = cmd.CreateParameter(); pSearch.ParameterName = "@Search"; pSearch.Value = (object?)search ?? DBNull.Value; pSearch.DbType = DbType.String; cmd.Parameters.Add(pSearch);
            var pType = cmd.CreateParameter(); pType.ParameterName = "@Type"; pType.Value = (object?)type ?? DBNull.Value; pType.DbType = DbType.Int32; cmd.Parameters.Add(pType);
            var pDateFrom = cmd.CreateParameter(); pDateFrom.ParameterName = "@DateFrom"; pDateFrom.Value = (object?)dateFrom ?? DBNull.Value; pDateFrom.DbType = DbType.DateTime2; cmd.Parameters.Add(pDateFrom);
            var pDateTo = cmd.CreateParameter(); pDateTo.ParameterName = "@DateTo"; pDateTo.Value = (object?)dateTo ?? DBNull.Value; pDateTo.DbType = DbType.DateTime2; cmd.Parameters.Add(pDateTo);
            var pSortBy = cmd.CreateParameter(); pSortBy.ParameterName = "@SortBy"; pSortBy.Value = sortBy ?? "CreatedAt"; pSortBy.DbType = DbType.String; cmd.Parameters.Add(pSortBy);
            var pSortOrder = cmd.CreateParameter(); pSortOrder.ParameterName = "@SortOrder"; pSortOrder.Value = sortOrder ?? "DESC"; pSortOrder.DbType = DbType.String; cmd.Parameters.Add(pSortOrder);
            var pPageNumber = cmd.CreateParameter(); pPageNumber.ParameterName = "@PageNumber"; pPageNumber.Value = pageNumber; pPageNumber.DbType = DbType.Int32; cmd.Parameters.Add(pPageNumber);
            var pPageSize = cmd.CreateParameter(); pPageSize.ParameterName = "@PageSize"; pPageSize.Value = pageSize; pPageSize.DbType = DbType.Int32; cmd.Parameters.Add(pPageSize);

            using var reader = await cmd.ExecuteReaderAsync();

            // First result: total count
            if (await reader.ReadAsync())
            {
                total = reader.GetInt32(0);
            }

            // Move to next result set: rows
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    var tx = new Transaction
                    {
                        Id = reader.GetInt32(0),
                        WalletId = reader.GetInt32(1),
                        Type = (PaySphere.BuildingBlocks.Enums.TransactionType)reader.GetInt32(2),
                        Amount = reader.GetDecimal(3),
                        BalanceBefore = reader.GetDecimal(4),
                        BalanceAfter = reader.GetDecimal(5),
                        Reference = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                        CreatedAt = reader.GetDateTime(8),
                        UpdatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
                    };

                    results.Add(tx);
                }
            }
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }

        return (results, total);
    }
}
