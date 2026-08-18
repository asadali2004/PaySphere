using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PaySphere.WalletService.Data;
using PaySphere.WalletService.Entities;
using PaySphere.WalletService.Repositories;
using PaySphere.BuildingBlocks.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaySphere.WalletService.Tests
{
    [TestFixture]
    /// <summary>
    /// Contains integration-style tests for TransactionRepository covering
    /// filtering, sorting and pagination behavior using an in-memory SQLite DB.
    /// </summary>
    public class TransactionRepositoryTests
    {
        private SqliteConnection _connection = null!;
        private WalletDbContext _dbContext = null!;
        private TransactionRepository _repository = null!;

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

            _repository = new TransactionRepository(_dbContext);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _dbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }

        [Test]
        public async Task GetByWalletIdWithFiltersAsync_SearchMatchesReferenceOrDescription()
        {
            var wallet = new Wallet { UserId = 1, Balance = 0m, Status = WalletStatus.Active, CreatedAt = DateTime.UtcNow };
            await _dbContext.Wallets.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();

            var tx1 = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 100m, BalanceBefore = 0m, BalanceAfter = 100m, Reference = "REF-ABC", Description = "monthly top up", CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
            var tx2 = new Transaction { WalletId = wallet.Id, Type = TransactionType.Withdrawal, Amount = 50m, BalanceBefore = 100m, BalanceAfter = 50m, Reference = "PAY-123", Description = "payment invoice", CreatedAt = DateTime.UtcNow.AddMinutes(-5) };

            await _dbContext.Transactions.AddRangeAsync(tx1, tx2);
            await _dbContext.SaveChangesAsync();

            var (items, total) = await _repository.GetByWalletIdWithFiltersAsync(wallet.Id, "payment", null, null, null, "createdAt", "desc", 1, 10);

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items.Single().Reference.Should().Be("PAY-123");
        }

        [Test]
        public async Task GetByWalletIdWithFiltersAsync_TypeFilterWorks()
        {
            var wallet = new Wallet { UserId = 2, Balance = 0m, Status = WalletStatus.Active, CreatedAt = DateTime.UtcNow };
            await _dbContext.Wallets.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();

            var tx1 = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 10m, BalanceBefore = 0m, BalanceAfter = 10m, Reference = "A", CreatedAt = DateTime.UtcNow.AddMinutes(-3) };
            var tx2 = new Transaction { WalletId = wallet.Id, Type = TransactionType.Withdrawal, Amount = 5m, BalanceBefore = 10m, BalanceAfter = 5m, Reference = "B", CreatedAt = DateTime.UtcNow.AddMinutes(-2) };

            await _dbContext.Transactions.AddRangeAsync(tx1, tx2);
            await _dbContext.SaveChangesAsync();

            var (items, total) = await _repository.GetByWalletIdWithFiltersAsync(wallet.Id, null, (int)TransactionType.TopUp, null, null, "createdAt", "desc", 1, 10);

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items.Single().Type.Should().Be(TransactionType.TopUp);
        }

        [Test]
        public async Task GetByWalletIdWithFiltersAsync_DateRangeFilterWorks()
        {
            var wallet = new Wallet { UserId = 3, Balance = 0m, Status = WalletStatus.Active, CreatedAt = DateTime.UtcNow };
            await _dbContext.Wallets.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var old = now.AddDays(-10);
            var mid = now.AddDays(-2);
            var recent = now.AddDays(-1);

            var txOld = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 10m, BalanceBefore = 0m, BalanceAfter = 10m, Reference = "OLD", CreatedAt = old };
            var txMid = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 20m, BalanceBefore = 10m, BalanceAfter = 30m, Reference = "MID", CreatedAt = mid };
            var txRecent = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 30m, BalanceBefore = 30m, BalanceAfter = 60m, Reference = "REC", CreatedAt = recent };

            await _dbContext.Transactions.AddRangeAsync(txOld, txMid, txRecent);
            await _dbContext.SaveChangesAsync();

            var (items, total) = await _repository.GetByWalletIdWithFiltersAsync(wallet.Id, null, null, now.AddDays(-3), now, "createdAt", "desc", 1, 10);

            total.Should().Be(2);
            items.Should().HaveCount(2);
            items.Select(x => x.Reference).Should().Contain(new[] { "MID", "REC" });
        }

        [Test]
        public async Task GetByWalletIdWithFiltersAsync_SortByAmountWorks()
        {
            var wallet = new Wallet { UserId = 4, Balance = 0m, Status = WalletStatus.Active, CreatedAt = DateTime.UtcNow };
            await _dbContext.Wallets.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();

            var tx1 = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 300m, BalanceBefore = 0m, BalanceAfter = 300m, Reference = "X", CreatedAt = DateTime.UtcNow.AddMinutes(-3) };
            var tx2 = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 100m, BalanceBefore = 300m, BalanceAfter = 400m, Reference = "Y", CreatedAt = DateTime.UtcNow.AddMinutes(-2) };
            var tx3 = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 200m, BalanceBefore = 400m, BalanceAfter = 600m, Reference = "Z", CreatedAt = DateTime.UtcNow.AddMinutes(-1) };

            await _dbContext.Transactions.AddRangeAsync(tx1, tx2, tx3);
            await _dbContext.SaveChangesAsync();

            var (ascItems, _) = await _repository.GetByWalletIdWithFiltersAsync(wallet.Id, null, null, null, null, "amount", "asc", 1, 10);
            ascItems.Select(x => x.Amount).Should().BeInAscendingOrder();

            var (descItems, _) = await _repository.GetByWalletIdWithFiltersAsync(wallet.Id, null, null, null, null, "amount", "desc", 1, 10);
            descItems.Select(x => x.Amount).Should().BeInDescendingOrder();
        }

        [Test]
        public async Task GetByWalletIdWithFiltersAsync_SortByCreatedAtWorks()
        {
            var wallet = new Wallet { UserId = 5, Balance = 0m, Status = WalletStatus.Active, CreatedAt = DateTime.UtcNow };
            await _dbContext.Wallets.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();

            var a = DateTime.UtcNow.AddMinutes(-30);
            var b = DateTime.UtcNow.AddMinutes(-20);
            var c = DateTime.UtcNow.AddMinutes(-10);

            var tx1 = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 10m, BalanceBefore = 0m, BalanceAfter = 10m, Reference = "1", CreatedAt = a };
            var tx2 = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 20m, BalanceBefore = 10m, BalanceAfter = 30m, Reference = "2", CreatedAt = b };
            var tx3 = new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 30m, BalanceBefore = 30m, BalanceAfter = 60m, Reference = "3", CreatedAt = c };

            await _dbContext.Transactions.AddRangeAsync(tx1, tx2, tx3);
            await _dbContext.SaveChangesAsync();

            var (asc, _) = await _repository.GetByWalletIdWithFiltersAsync(wallet.Id, null, null, null, null, "createdAt", "asc", 1, 10);
            asc.Select(x => x.CreatedAt).Should().BeInAscendingOrder();

            var (desc, _) = await _repository.GetByWalletIdWithFiltersAsync(wallet.Id, null, null, null, null, "createdAt", "desc", 1, 10);
            desc.Select(x => x.CreatedAt).Should().BeInDescendingOrder();
        }

        [Test]
        public async Task GetByWalletIdWithFiltersAsync_PaginationWorks()
        {
            var wallet = new Wallet { UserId = 6, Balance = 0m, Status = WalletStatus.Active, CreatedAt = DateTime.UtcNow };
            await _dbContext.Wallets.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();

            var list = new List<Transaction>();
            for (int i = 1; i <= 5; i++)
            {
                list.Add(new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = i * 10, BalanceBefore = 0m, BalanceAfter = i * 10, Reference = i.ToString(), CreatedAt = DateTime.UtcNow.AddMinutes(-i) });
            }

            await _dbContext.Transactions.AddRangeAsync(list);
            await _dbContext.SaveChangesAsync();

            var (pageItems, total) = await _repository.GetByWalletIdWithFiltersAsync(wallet.Id, null, null, null, null, "createdAt", "desc", 2, 2);

            total.Should().Be(5);
            pageItems.Should().HaveCount(2);
        }

        [Test]
        public async Task GetByWalletIdWithFiltersAsync_UnsupportedSortByDefaultsToCreatedAtDesc()
        {
            var wallet = new Wallet { UserId = 7, Balance = 0m, Status = WalletStatus.Active, CreatedAt = DateTime.UtcNow };
            await _dbContext.Wallets.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();

            var a = DateTime.UtcNow.AddMinutes(-3);
            var b = DateTime.UtcNow.AddMinutes(-2);
            var c = DateTime.UtcNow.AddMinutes(-1);

            await _dbContext.Transactions.AddRangeAsync(
                new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 1m, BalanceBefore = 0m, BalanceAfter = 1m, Reference = "A", CreatedAt = a },
                new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 2m, BalanceBefore = 1m, BalanceAfter = 3m, Reference = "B", CreatedAt = b },
                new Transaction { WalletId = wallet.Id, Type = TransactionType.TopUp, Amount = 3m, BalanceBefore = 3m, BalanceAfter = 6m, Reference = "C", CreatedAt = c }
            );
            await _dbContext.SaveChangesAsync();

            var (items, _) = await _repository.GetByWalletIdWithFiltersAsync(wallet.Id, null, null, null, null, "invalid", "invalid", 1, 10);

            items.Select(x => x.CreatedAt).Should().BeInDescendingOrder();
        }
    }
}
