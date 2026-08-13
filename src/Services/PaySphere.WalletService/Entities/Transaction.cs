using PaySphere.BuildingBlocks.Base;
using PaySphere.BuildingBlocks.Enums;

namespace PaySphere.WalletService.Entities;

/// <summary>
/// Represents a ledger entry for a wallet operation. The service records both BalanceBefore and
/// BalanceAfter so transaction history is immutable and audit-friendly.
/// </summary>
public class Transaction : BaseEntity
{
    // FK to Wallet entity.
    public int WalletId { get; set; }

    // Type distinguishes top-ups, withdrawals and transfer debit/credit entries.
    public TransactionType Type { get; set; }

    // Amount involved in the transaction. Positive for both debit and credit entries; Type defines semantics.
    public decimal Amount { get; set; }

    // Balance immediately before applying this transaction.
    public decimal BalanceBefore { get; set; }

    // Balance immediately after applying this transaction.
    public decimal BalanceAfter { get; set; }

    // Reference is used to link related transactions (e.g. debit/credit pair for a transfer).
    public string? Reference { get; set; }

    public string? Description { get; set; }

    public Wallet Wallet { get; set; } = null!;
}
