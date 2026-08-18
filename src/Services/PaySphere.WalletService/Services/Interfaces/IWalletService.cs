using PaySphere.BuildingBlocks.Pagination;
using PaySphere.BuildingBlocks.Responses;
using PaySphere.WalletService.DTOs.Requests;
using PaySphere.WalletService.DTOs.Responses;

namespace PaySphere.WalletService.Services.Interfaces;

public interface IWalletService
{
    /// <summary>
    /// Creates a wallet for the specified user. Business rule: a user may only have one wallet.
    /// </summary>
    Task<WalletResponse> CreateWalletAsync(int userId);

    /// <summary>
    /// Retrieves the wallet for the specified user.
    /// </summary>
    Task<WalletResponse> GetWalletAsync(int userId);

    /// <summary>
    /// Returns the user's wallet balance.
    /// </summary>
    Task<WalletBalanceResponse> GetBalanceAsync(int userId);

    /// <summary>
    /// Credits the user's wallet. The service handles validation and transaction recording.
    /// </summary>
    Task<WalletResponse> TopUpAsync(int userId, TopUpRequest request);

    /// <summary>
    /// Debits the user's wallet after validating sufficient balance and status.
    /// </summary>
    Task<WalletResponse> WithdrawAsync(int userId, WithdrawRequest request);

    /// <summary>
    /// Transfers funds from sender to receiver atomically. Ensures receivers are validated
    /// via the AuthService and creates paired debit/credit transaction records.
    /// </summary>
    Task<WalletResponse> TransferAsync(int senderUserId, TransferRequest request);

    /// <summary>
    /// Retrieves paginated transaction history for the user's wallet supporting filters and sorting.
    /// </summary>
    Task<PagedResponse<TransactionResponse>> GetTransactionsAsync(
        int userId,
        PaginationRequest request,
        string? search = null,
        string? type = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string sortBy = "createdAt",
        string sortOrder = "desc");
}
