using Microsoft.EntityFrameworkCore;
using PaySphere.BuildingBlocks.Enums;
using PaySphere.BuildingBlocks.Exceptions;
using PaySphere.BuildingBlocks.Pagination;
using PaySphere.BuildingBlocks.Responses;
using PaySphere.WalletService.Data;
using PaySphere.WalletService.DTOs.Requests;
using PaySphere.WalletService.DTOs.Responses;
using PaySphere.WalletService.Entities;
using PaySphere.WalletService.Exceptions;
using PaySphere.WalletService.Repositories.Interfaces;
using PaySphere.WalletService.Services.Interfaces;
using PaySphere.WalletService.Validators;

namespace PaySphere.WalletService.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAuthServiceClient _authServiceClient;
    private readonly WalletDbContext _dbContext;

    public WalletService(
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        IAuthServiceClient authServiceClient,
        WalletDbContext dbContext)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _authServiceClient = authServiceClient;
        _dbContext = dbContext;
    }

    public async Task<WalletResponse> CreateWalletAsync(int userId)
    {
        if (await _walletRepository.ExistsByUserIdAsync(userId))
        {
            throw new WalletAlreadyExistsException();
        }

        var wallet = new Wallet
        {
            UserId = userId,
            Balance = 0m,
            Status = WalletStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.AddAsync(wallet);
        await _walletRepository.SaveChangesAsync();

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse> GetWalletAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);

        if (wallet is null)
        {
            throw new WalletNotFoundException();
        }

        EnsureWalletIsActive(wallet);

        return MapToResponse(wallet);
    }

    public async Task<WalletBalanceResponse> GetBalanceAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);

        if (wallet is null)
        {
            throw new WalletNotFoundException();
        }

        EnsureWalletIsActive(wallet);

        return new WalletBalanceResponse
        {
            Balance = wallet.Balance
        };
    }

    public Task<WalletResponse> TopUpAsync(int userId, TopUpRequest request)
    {
        TopUpRequestValidator.Validate(request);

        return ExecuteFinancialOperationAsync(
            userId,
            request.Amount,
            TransactionType.TopUp,
            "Wallet top-up",
            (balance, amount) => balance + amount);
    }

    public Task<WalletResponse> WithdrawAsync(int userId, WithdrawRequest request)
    {
        WithdrawRequestValidator.Validate(request);

        return ExecuteFinancialOperationAsync(
            userId,
            request.Amount,
            TransactionType.Withdrawal,
            "Wallet withdrawal",
            (balance, amount) => balance - amount,
            ensureSufficientBalance: true);
    }

    public async Task<WalletResponse> TransferAsync(int senderUserId, TransferRequest request)
    {
        TransferRequestValidator.Validate(request);

        if (senderUserId == request.ReceiverUserId)
        {
            throw new BaseException("Sender and receiver cannot be the same.");
        }

        var receiverValidation = await _authServiceClient.ValidateReceiverAsync(request.ReceiverUserId);
        if (!receiverValidation.Exists || !receiverValidation.IsActive)
        {
            throw new ReceiverNotFoundException();
        }

        var senderWallet = await _walletRepository.GetByUserIdAsync(senderUserId);
        if (senderWallet is null)
        {
            throw new WalletNotFoundException();
        }

        EnsureWalletIsActive(senderWallet);

        var receiverWallet = await _walletRepository.GetByUserIdAsync(request.ReceiverUserId);
        if (receiverWallet is null)
        {
            throw new ReceiverNotFoundException();
        }

        EnsureWalletIsActive(receiverWallet);

        if (senderWallet.Balance < request.Amount)
        {
            throw new InsufficientBalanceException();
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var senderBalanceBefore = senderWallet.Balance;
        var receiverBalanceBefore = receiverWallet.Balance;
        var senderBalanceAfter = senderBalanceBefore - request.Amount;
        var receiverBalanceAfter = receiverBalanceBefore + request.Amount;
        var reference = await GenerateUniqueReferenceAsync();

        senderWallet.Balance = senderBalanceAfter;
        senderWallet.UpdatedAt = DateTime.UtcNow;
        receiverWallet.Balance = receiverBalanceAfter;
        receiverWallet.UpdatedAt = DateTime.UtcNow;

        _walletRepository.Update(senderWallet);
        _walletRepository.Update(receiverWallet);

        await _transactionRepository.AddAsync(new Transaction
        {
            WalletId = senderWallet.Id,
            Type = TransactionType.TransferDebit,
            Amount = request.Amount,
            BalanceBefore = senderBalanceBefore,
            BalanceAfter = senderBalanceAfter,
            Reference = reference,
            Description = request.Description ?? "Transfer debit",
            CreatedAt = DateTime.UtcNow
        });

        await _walletRepository.SaveChangesAsync();

        await _transactionRepository.AddAsync(new Transaction
        {
            WalletId = receiverWallet.Id,
            Type = TransactionType.TransferCredit,
            Amount = request.Amount,
            BalanceBefore = receiverBalanceBefore,
            BalanceAfter = receiverBalanceAfter,
            Reference = reference,
            Description = request.Description ?? "Transfer credit",
            CreatedAt = DateTime.UtcNow
        });

        await _walletRepository.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToResponse(senderWallet);
    }

    public async Task<PagedResponse<TransactionResponse>> GetTransactionsAsync(
        int userId,
        PaginationRequest request,
        string? search = null,
        string? type = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string sortBy = "createdAt",
        string sortOrder = "desc")
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);

        if (wallet is null)
        {
            throw new WalletNotFoundException();
        }

        // parse type if provided
        int? typeValue = null;
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(type, true, out PaySphere.BuildingBlocks.Enums.TransactionType parsedType))
        {
            typeValue = (int)parsedType;
        }

        // Use repository LINQ-based method
        var (transactions, totalRecords) = await _transactionRepository.GetByWalletIdWithFiltersAsync(
            wallet.Id,
            search,
            typeValue,
            dateFrom,
            dateTo,
            sortBy,
            sortOrder,
            request.PageNumber,
            request.PageSize);

        var page = transactions.Select(MapToTransactionResponse).ToList();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        return new PagedResponse<TransactionResponse>
        {
            Success = true,
            Message = "Transactions retrieved successfully.",
            Data = page,
            Errors = new List<string>(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    private async Task<WalletResponse> ExecuteFinancialOperationAsync(
        int userId,
        decimal amount,
        TransactionType transactionType,
        string description,
        Func<decimal, decimal, decimal> balanceCalculator,
        bool ensureSufficientBalance = false)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);

        if (wallet is null)
        {
            throw new WalletNotFoundException();
        }

        EnsureWalletIsActive(wallet);

        if (ensureSufficientBalance && wallet.Balance < amount)
        {
            throw new InsufficientBalanceException();
        }

        var balanceBefore = wallet.Balance;
        var balanceAfter = balanceCalculator(balanceBefore, amount);
        var reference = await GenerateUniqueReferenceAsync();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        wallet.Balance = balanceAfter;
        wallet.UpdatedAt = DateTime.UtcNow;
        _walletRepository.Update(wallet);

        var audit = new Transaction
        {
            WalletId = wallet.Id,
            Type = transactionType,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Reference = reference,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(audit);
        await _walletRepository.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToResponse(wallet);
    }

    private async Task<string> GenerateUniqueReferenceAsync()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var reference = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..23].ToUpperInvariant();

            if (await _transactionRepository.GetByReferenceAsync(reference) is null)
            {
                return reference;
            }
        }

        return $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".ToUpperInvariant();
    }

    private static void EnsureWalletIsActive(Wallet wallet)
    {
        if (wallet.Status != WalletStatus.Active)
        {
            throw new WalletNotActiveException();
        }
    }

    private static WalletResponse MapToResponse(Wallet wallet)
    {
        return new WalletResponse
        {
            Id = wallet.Id,
            UserId = wallet.UserId,
            Balance = wallet.Balance,
            Status = wallet.Status,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }

    private static TransactionResponse MapToTransactionResponse(Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            Type = transaction.Type,
            Amount = transaction.Amount,
            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,
            Reference = transaction.Reference,
            Description = transaction.Description,
            CreatedAt = transaction.CreatedAt
        };
    }
}
