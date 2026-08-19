using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PaySphere.WalletService.Data;
using PaySphere.WalletService.Entities;
using PaySphere.WalletService.Repositories.Interfaces;
using PaySphere.WalletService.Services.Interfaces;
using WalletServiceImpl = PaySphere.WalletService.Services.WalletService;
using PaySphere.WalletService.DTOs.Requests;
using PaySphere.WalletService.Exceptions;

namespace PaySphere.WalletService.Tests
{
    // Simplified, fresher-style NUnit tests (5 essential tests)
    public class WalletServiceTests
    {
        private SqliteConnection _connection = null!;
        private WalletDbContext _dbContext = null!;
        private Mock<IWalletRepository> _walletRepo = null!;
        private Mock<ITransactionRepository> _txRepo = null!;
        private Mock<IAuthServiceClient> _authClient = null!;
        private WalletServiceImpl _service = null!;

        [SetUp]
        public async System.Threading.Tasks.Task SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            await _connection.OpenAsync();

            var options = new DbContextOptionsBuilder<WalletDbContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new WalletDbContext(options);
            await _dbContext.Database.EnsureCreatedAsync();

            _walletRepo = new Mock<IWalletRepository>();
            _txRepo = new Mock<ITransactionRepository>();
            _authClient = new Mock<IAuthServiceClient>();

            _service = new WalletServiceImpl(_walletRepo.Object, _txRepo.Object, _authClient.Object, _dbContext);
        }

        [TearDown]
        public async System.Threading.Tasks.Task TearDown()
        {
            await _dbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }

        [Test]
        public async System.Threading.Tasks.Task CreateWallet_Succeeds()
        {
            var userId = 1;
            _walletRepo.Setup(r => r.ExistsByUserIdAsync(userId)).ReturnsAsync(false);
            _walletRepo.Setup(r => r.AddAsync(It.IsAny<Wallet>())).Returns(System.Threading.Tasks.Task.CompletedTask)
                .Callback<Wallet>(w => w.Id = 100);
            _walletRepo.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

            var result = await _service.CreateWalletAsync(userId);

            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Balance, Is.EqualTo(0m));
        }

        [Test]
        public void CreateWallet_WhenExists_Throws()
        {
            var userId = 2;
            _walletRepo.Setup(r => r.ExistsByUserIdAsync(userId)).ReturnsAsync(true);

            Assert.That(async () => await _service.CreateWalletAsync(userId), Throws.TypeOf<WalletAlreadyExistsException>());
        }

        [Test]
        public async System.Threading.Tasks.Task GetBalance_ReturnsValue()
        {
            var userId = 3;
            var wallet = new Wallet { UserId = userId, Balance = 42.5m, Status = PaySphere.BuildingBlocks.Enums.WalletStatus.Active };
            _walletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

            var resp = await _service.GetBalanceAsync(userId);

            Assert.That(resp.Balance, Is.EqualTo(42.5m));
        }

        [Test]
        public async System.Threading.Tasks.Task TopUp_IncreasesBalance()
        {
            var userId = 4;
            var wallet = new Wallet { UserId = userId, Balance = 10m, Status = PaySphere.BuildingBlocks.Enums.WalletStatus.Active };
            _walletRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
            _walletRepo.Setup(r => r.Update(It.IsAny<Wallet>()));
            _walletRepo.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

            var req = new TopUpRequest { Amount = 5m };
            var result = await _service.TopUpAsync(userId, req);

            Assert.That(result.Balance, Is.EqualTo(15m));
        }

        [Test]
        public void Transfer_InsufficientBalance_Throws()
        {
            var sender = 5;
            var receiver = 6;
            var senderWallet = new Wallet { UserId = sender, Balance = 10m, Status = PaySphere.BuildingBlocks.Enums.WalletStatus.Active };
            var receiverWallet = new Wallet { UserId = receiver, Balance = 0m, Status = PaySphere.BuildingBlocks.Enums.WalletStatus.Active };

            _authClient.Setup(a => a.ValidateReceiverAsync(receiver)).ReturnsAsync(new DTOs.Responses.ReceiverValidationResponse { UserId = receiver, Exists = true, IsActive = true });
            _walletRepo.Setup(r => r.GetByUserIdAsync(sender)).ReturnsAsync(senderWallet);
            _walletRepo.Setup(r => r.GetByUserIdAsync(receiver)).ReturnsAsync(receiverWallet);

            Assert.That(async () => await _service.TransferAsync(sender, new PaySphere.WalletService.DTOs.Requests.TransferRequest { ReceiverUserId = receiver, Amount = 50m }),
                Throws.TypeOf<PaySphere.WalletService.Exceptions.InsufficientBalanceException>());
        }
    }
}
