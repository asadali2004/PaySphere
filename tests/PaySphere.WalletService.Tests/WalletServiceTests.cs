using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
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
using WalletServiceImpl = PaySphere.WalletService.Services.WalletService;

namespace PaySphere.WalletService.Tests;

public class WalletServiceTests
{
    private SqliteConnection _connection = null!;
    private WalletDbContext _dbContext = null!;
    private Mock<IWalletRepository> _walletRepositoryMock = null!;
    private Mock<ITransactionRepository> _transactionRepositoryMock = null!;
    private Mock<IAuthServiceClient> _authServiceClientMock = null!;
    private WalletServiceImpl _walletService = null!;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<WalletDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new WalletDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _walletRepositoryMock = new Mock<IWalletRepository>(MockBehavior.Strict);
        _transactionRepositoryMock = new Mock<ITransactionRepository>(MockBehavior.Strict);
        _authServiceClientMock = new Mock<IAuthServiceClient>(MockBehavior.Strict);
        _walletService = new WalletServiceImpl(
            _walletRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            _authServiceClientMock.Object,
            _dbContext);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task CreateWalletAsync_ShouldCreateWalletSuccessfully()
    {
        var userId = 5;
        Wallet? addedWallet = null;

        _walletRepositoryMock.Setup(x => x.ExistsByUserIdAsync(userId)).ReturnsAsync(false);
        _walletRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Wallet>()))
            .Callback<Wallet>(wallet =>
            {
                wallet.Id = 10;
                addedWallet = wallet;
            })
            .Returns(Task.CompletedTask);
        _walletRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _walletService.CreateWalletAsync(userId);

        result.Id.Should().Be(10);
        result.UserId.Should().Be(userId);
        result.Balance.Should().Be(0m);
        result.Status.Should().Be(WalletStatus.Active);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        addedWallet.Should().NotBeNull();

        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task CreateWalletAsync_ShouldRejectDuplicateWallet()
    {
        var userId = 5;

        _walletRepositoryMock.Setup(x => x.ExistsByUserIdAsync(userId)).ReturnsAsync(true);

        var act = async () => await _walletService.CreateWalletAsync(userId);

        await act.Should().ThrowAsync<WalletAlreadyExistsException>()
            .WithMessage("Wallet already exists.");

        _walletRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Wallet>()), Times.Never);
        _walletRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task GetWalletAsync_ShouldReturnWallet()
    {
        var userId = 5;
        var wallet = CreateWallet(userId, 150.25m, WalletStatus.Active);

        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

        var result = await _walletService.GetWalletAsync(userId);

        result.UserId.Should().Be(userId);
        result.Balance.Should().Be(150.25m);
        result.Status.Should().Be(WalletStatus.Active);
        result.CreatedAt.Should().Be(wallet.CreatedAt);

        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task GetWalletAsync_ShouldThrowWhenWalletNotFound()
    {
        var userId = 5;

        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((Wallet?)null);

        var act = async () => await _walletService.GetWalletAsync(userId);

        await act.Should().ThrowAsync<WalletNotFoundException>()
            .WithMessage("Wallet not found.");

        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task GetBalanceAsync_ShouldReturnCurrentBalance()
    {
        var userId = 5;
        var wallet = CreateWallet(userId, 999.99m, WalletStatus.Active);

        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

        var result = await _walletService.GetBalanceAsync(userId);

        result.Balance.Should().Be(999.99m);

        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task GetBalanceAsync_ShouldRejectInactiveWallet()
    {
        var userId = 5;
        var wallet = CreateWallet(userId, 999.99m, WalletStatus.Frozen);

        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

        var act = async () => await _walletService.GetBalanceAsync(userId);

        await act.Should().ThrowAsync<WalletNotActiveException>()
            .WithMessage("Wallet is not active.");

        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task TopUpAsync_ShouldIncreaseBalanceAndCreateTransaction()
    {
        var userId = 5;
        var wallet = CreateWallet(userId, 1000m, WalletStatus.Active);
        Transaction? capturedTransaction = null;

        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
        _walletRepositoryMock.Setup(x => x.Update(wallet));
        _walletRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _transactionRepositoryMock.Setup(x => x.GetByReferenceAsync(It.IsAny<string>())).ReturnsAsync((Transaction?)null);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(transaction => capturedTransaction = transaction)
            .Returns(Task.CompletedTask);

        var result = await _walletService.TopUpAsync(userId, new TopUpRequest { Amount = 500.50m });

        result.Balance.Should().Be(1500.50m);
        wallet.Balance.Should().Be(1500.50m);
        result.UpdatedAt.Should().NotBeNull();
        capturedTransaction.Should().NotBeNull();
        capturedTransaction!.Type.Should().Be(TransactionType.TopUp);
        capturedTransaction.Amount.Should().Be(500.50m);
        capturedTransaction.BalanceBefore.Should().Be(1000m);
        capturedTransaction.BalanceAfter.Should().Be(1500.50m);
        capturedTransaction.Reference.Should().StartWith("TXN-");
        capturedTransaction.Description.Should().Be("Wallet top-up");

        _walletRepositoryMock.VerifyAll();
        _transactionRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task WithdrawAsync_ShouldDecreaseBalanceAndCreateTransaction()
    {
        var userId = 5;
        var wallet = CreateWallet(userId, 1500m, WalletStatus.Active);
        Transaction? capturedTransaction = null;

        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
        _walletRepositoryMock.Setup(x => x.Update(wallet));
        _walletRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _transactionRepositoryMock.Setup(x => x.GetByReferenceAsync(It.IsAny<string>())).ReturnsAsync((Transaction?)null);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(transaction => capturedTransaction = transaction)
            .Returns(Task.CompletedTask);

        var result = await _walletService.WithdrawAsync(userId, new WithdrawRequest { Amount = 200m });

        result.Balance.Should().Be(1300m);
        wallet.Balance.Should().Be(1300m);
        capturedTransaction.Should().NotBeNull();
        capturedTransaction!.Type.Should().Be(TransactionType.Withdrawal);
        capturedTransaction.Amount.Should().Be(200m);
        capturedTransaction.BalanceBefore.Should().Be(1500m);
        capturedTransaction.BalanceAfter.Should().Be(1300m);
        capturedTransaction.Reference.Should().StartWith("TXN-");
        capturedTransaction.Description.Should().Be("Wallet withdrawal");

        _walletRepositoryMock.VerifyAll();
        _transactionRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task TopUpAsync_ShouldRejectInvalidAmount()
    {
        var act = async () => await _walletService.TopUpAsync(5, new TopUpRequest { Amount = 0m });

        await act.Should().ThrowAsync<InvalidTransactionAmountException>()
            .WithMessage("Invalid transaction amount.");
    }

    [Test]
    public async Task WithdrawAsync_ShouldRejectAmountWithMoreThanTwoDecimals()
    {
        var act = async () => await _walletService.WithdrawAsync(5, new WithdrawRequest { Amount = 10.123m });

        await act.Should().ThrowAsync<InvalidTransactionAmountException>()
            .WithMessage("Invalid transaction amount.");
    }

    [Test]
    public async Task WithdrawAsync_ShouldRejectInsufficientBalance()
    {
        var userId = 5;
        var wallet = CreateWallet(userId, 1000m, WalletStatus.Active);

        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

        var act = async () => await _walletService.WithdrawAsync(userId, new WithdrawRequest { Amount = 1500m });

        await act.Should().ThrowAsync<InsufficientBalanceException>()
            .WithMessage("Insufficient wallet balance.");

        _walletRepositoryMock.Verify(x => x.Update(It.IsAny<Wallet>()), Times.Never);
        _walletRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _transactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Never);
        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task TopUpAsync_ShouldRejectInactiveWallet()
    {
        var userId = 5;
        var wallet = CreateWallet(userId, 1000m, WalletStatus.Frozen);

        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

        var act = async () => await _walletService.TopUpAsync(userId, new TopUpRequest { Amount = 100m });

        await act.Should().ThrowAsync<WalletNotActiveException>()
            .WithMessage("Wallet is not active.");

        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task TransferAsync_ShouldMoveFundsAndCreateDebitAndCreditTransactions()
    {
        var senderUserId = 5;
        var receiverUserId = 10;
        var senderWallet = CreateWallet(senderUserId, 1000m, WalletStatus.Active);
        var receiverWallet = CreateWallet(receiverUserId, 500m, WalletStatus.Active);
        Transaction? debitTransaction = null;
        Transaction? creditTransaction = null;

        _authServiceClientMock.Setup(x => x.ValidateReceiverAsync(receiverUserId))
            .ReturnsAsync(new ReceiverValidationResponse
            {
                UserId = receiverUserId,
                Exists = true,
                IsActive = true
            });
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(senderUserId)).ReturnsAsync(senderWallet);
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(receiverUserId)).ReturnsAsync(receiverWallet);
        _walletRepositoryMock.Setup(x => x.Update(senderWallet));
        _walletRepositoryMock.Setup(x => x.Update(receiverWallet));
        _walletRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _transactionRepositoryMock.Setup(x => x.GetByReferenceAsync(It.IsAny<string>())).ReturnsAsync((Transaction?)null);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.Is<Transaction>(t => t.Type == TransactionType.TransferDebit)))
            .Callback<Transaction>(transaction => debitTransaction = transaction)
            .Returns(Task.CompletedTask);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.Is<Transaction>(t => t.Type == TransactionType.TransferCredit)))
            .Callback<Transaction>(transaction => creditTransaction = transaction)
            .Returns(Task.CompletedTask);

        var result = await _walletService.TransferAsync(senderUserId, new TransferRequest
        {
            ReceiverUserId = receiverUserId,
            Amount = 300m,
            Description = "Payment"
        });

        result.Balance.Should().Be(700m);
        senderWallet.Balance.Should().Be(700m);
        receiverWallet.Balance.Should().Be(800m);
        debitTransaction.Should().NotBeNull();
        creditTransaction.Should().NotBeNull();
        debitTransaction!.Type.Should().Be(TransactionType.TransferDebit);
        creditTransaction!.Type.Should().Be(TransactionType.TransferCredit);
        debitTransaction.Reference.Should().NotBeNullOrWhiteSpace();
        debitTransaction.Reference.Should().Be(creditTransaction.Reference);

        _authServiceClientMock.VerifyAll();
        _walletRepositoryMock.VerifyAll();
        _transactionRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task TransferAsync_ShouldRejectSelfTransfer()
    {
        var act = async () => await _walletService.TransferAsync(5, new TransferRequest
        {
            ReceiverUserId = 5,
            Amount = 50m,
            Description = "Self transfer"
        });

        await act.Should().ThrowAsync<BaseException>()
            .WithMessage("Sender and receiver cannot be the same.");

        _authServiceClientMock.Verify(x => x.ValidateReceiverAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task TransferAsync_ShouldRejectInvalidReceiver()
    {
        _authServiceClientMock.Setup(x => x.ValidateReceiverAsync(10))
            .ReturnsAsync(new ReceiverValidationResponse
            {
                UserId = 10,
                Exists = false,
                IsActive = false
            });

        var act = async () => await _walletService.TransferAsync(5, new TransferRequest
        {
            ReceiverUserId = 10,
            Amount = 50m,
            Description = "Payment"
        });

        await act.Should().ThrowAsync<ReceiverNotFoundException>()
            .WithMessage("Receiver not found.");

        _authServiceClientMock.VerifyAll();
    }

    [Test]
    public async Task TransferAsync_ShouldRejectReceiverWithoutWallet()
    {
        var senderUserId = 5;
        var receiverUserId = 10;
        var senderWallet = CreateWallet(senderUserId, 1000m, WalletStatus.Active);

        _authServiceClientMock.Setup(x => x.ValidateReceiverAsync(receiverUserId))
            .ReturnsAsync(new ReceiverValidationResponse
            {
                UserId = receiverUserId,
                Exists = true,
                IsActive = true
            });
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(senderUserId)).ReturnsAsync(senderWallet);
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(receiverUserId)).ReturnsAsync((Wallet?)null);

        var act = async () => await _walletService.TransferAsync(senderUserId, new TransferRequest
        {
            ReceiverUserId = receiverUserId,
            Amount = 50m,
            Description = "Payment"
        });

        await act.Should().ThrowAsync<ReceiverNotFoundException>()
            .WithMessage("Receiver not found.");

        _authServiceClientMock.VerifyAll();
        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task TransferAsync_ShouldRejectInsufficientBalance()
    {
        var senderUserId = 5;
        var receiverUserId = 10;
        var senderWallet = CreateWallet(senderUserId, 100m, WalletStatus.Active);
        var receiverWallet = CreateWallet(receiverUserId, 500m, WalletStatus.Active);

        _authServiceClientMock.Setup(x => x.ValidateReceiverAsync(receiverUserId))
            .ReturnsAsync(new ReceiverValidationResponse
            {
                UserId = receiverUserId,
                Exists = true,
                IsActive = true
            });
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(senderUserId)).ReturnsAsync(senderWallet);
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(receiverUserId)).ReturnsAsync(receiverWallet);

        var act = async () => await _walletService.TransferAsync(senderUserId, new TransferRequest
        {
            ReceiverUserId = receiverUserId,
            Amount = 300m,
            Description = "Payment"
        });

        await act.Should().ThrowAsync<InsufficientBalanceException>()
            .WithMessage("Insufficient wallet balance.");

        _walletRepositoryMock.Verify(x => x.Update(It.IsAny<Wallet>()), Times.Never);
        _walletRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _transactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Never);
        _authServiceClientMock.VerifyAll();
    }

    [Test]
    public async Task TransferAsync_ShouldRejectInactiveSender()
    {
        var senderUserId = 5;
        var receiverUserId = 10;
        var senderWallet = CreateWallet(senderUserId, 1000m, WalletStatus.Frozen);

        _authServiceClientMock.Setup(x => x.ValidateReceiverAsync(receiverUserId))
            .ReturnsAsync(new ReceiverValidationResponse
            {
                UserId = receiverUserId,
                Exists = true,
                IsActive = true
            });
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(senderUserId)).ReturnsAsync(senderWallet);

        var act = async () => await _walletService.TransferAsync(senderUserId, new TransferRequest
        {
            ReceiverUserId = receiverUserId,
            Amount = 50m,
            Description = "Payment"
        });

        await act.Should().ThrowAsync<WalletNotActiveException>()
            .WithMessage("Wallet is not active.");

        _authServiceClientMock.VerifyAll();
        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task TransferAsync_ShouldRollbackWhenCreditTransactionFails()
    {
        var senderUserId = 5;
        var receiverUserId = 10;
        var senderWallet = CreateWallet(senderUserId, 1000m, WalletStatus.Active);
        var receiverWallet = CreateWallet(receiverUserId, 500m, WalletStatus.Active);

        _authServiceClientMock.Setup(x => x.ValidateReceiverAsync(receiverUserId))
            .ReturnsAsync(new ReceiverValidationResponse
            {
                UserId = receiverUserId,
                Exists = true,
                IsActive = true
            });
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(senderUserId)).ReturnsAsync(senderWallet);
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(receiverUserId)).ReturnsAsync(receiverWallet);
        _walletRepositoryMock.Setup(x => x.Update(senderWallet));
        _walletRepositoryMock.Setup(x => x.Update(receiverWallet));
        _walletRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _transactionRepositoryMock.Setup(x => x.GetByReferenceAsync(It.IsAny<string>())).ReturnsAsync((Transaction?)null);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.Is<Transaction>(t => t.Type == TransactionType.TransferDebit)))
            .Returns(Task.CompletedTask);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.Is<Transaction>(t => t.Type == TransactionType.TransferCredit)))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var act = async () => await _walletService.TransferAsync(senderUserId, new TransferRequest
        {
            ReceiverUserId = receiverUserId,
            Amount = 300m,
            Description = "Payment"
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        _walletRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _authServiceClientMock.VerifyAll();
        _walletRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task GetTransactionsAsync_ShouldReturnPagedHistory()
    {
        var userId = 5;
        var wallet = CreateWallet(userId, 1000m, WalletStatus.Active);
        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 3,
                WalletId = wallet.Id,
                Type = TransactionType.Withdrawal,
                Amount = 50m,
                BalanceBefore = 1050m,
                BalanceAfter = 1000m,
                Reference = "TXN-20260812-000003",
                Description = "Withdrawal",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            },
            new()
            {
                Id = 2,
                WalletId = wallet.Id,
                Type = TransactionType.TopUp,
                Amount = 100m,
                BalanceBefore = 950m,
                BalanceAfter = 1050m,
                Reference = "TXN-20260812-000002",
                Description = "Top up",
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new()
            {
                Id = 1,
                WalletId = wallet.Id,
                Type = TransactionType.TopUp,
                Amount = 200m,
                BalanceBefore = 750m,
                BalanceAfter = 950m,
                Reference = "TXN-20260812-000001",
                Description = "Top up",
                CreatedAt = DateTime.UtcNow.AddMinutes(-3)
            }
        };

        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
        _transactionRepositoryMock.Setup(x => x.GetByWalletIdAsync(wallet.Id)).ReturnsAsync(transactions);

        var result = await _walletService.GetTransactionsAsync(userId, new PaginationRequest
        {
            PageNumber = 2,
            PageSize = 2
        });

        result.Success.Should().BeTrue();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.Data.Should().HaveCount(1);
        result.Data!.Single().Id.Should().Be(1);

        _walletRepositoryMock.VerifyAll();
        _transactionRepositoryMock.VerifyAll();
    }

    private static Wallet CreateWallet(int userId, decimal balance, WalletStatus status)
    {
        return new Wallet
        {
            Id = userId + 100,
            UserId = userId,
            Balance = balance,
            Status = status,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = null
        };
    }
}
