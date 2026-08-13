using PaySphere.BuildingBlocks.Pagination;
using PaySphere.BuildingBlocks.Responses;
using PaySphere.WalletService.DTOs.Requests;
using PaySphere.WalletService.DTOs.Responses;

namespace PaySphere.WalletService.Services.Interfaces;

public interface IWalletService
{
    Task<WalletResponse> CreateWalletAsync(int userId);

    Task<WalletResponse> GetWalletAsync(int userId);

    Task<WalletBalanceResponse> GetBalanceAsync(int userId);

    Task<WalletResponse> TopUpAsync(int userId, TopUpRequest request);

    Task<WalletResponse> WithdrawAsync(int userId, WithdrawRequest request);

    Task<WalletResponse> TransferAsync(int senderUserId, TransferRequest request);

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
