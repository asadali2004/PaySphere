using PaySphere.BuildingBlocks.Base;
using PaySphere.BuildingBlocks.Enums;

namespace PaySphere.WalletService.Entities;

public class Transaction : BaseEntity
{
    public int WalletId { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Reference { get; set; }

    public string? Description { get; set; }

    public Wallet Wallet { get; set; } = null!;
}
