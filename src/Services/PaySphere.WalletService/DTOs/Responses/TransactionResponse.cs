using PaySphere.BuildingBlocks.Enums;

namespace PaySphere.WalletService.DTOs.Responses;

/// <summary>
/// Represents a wallet transaction returned by the WalletService. Contains
/// debit/credit semantics, balances before/after and optional metadata.
/// </summary>
public class TransactionResponse
{
    /// <summary>
    /// Transaction primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Type of transaction (Debit/Credit) as an enum.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Wallet balance before the transaction was applied.
    /// </summary>
    public decimal BalanceBefore { get; set; }

    /// <summary>
    /// Wallet balance after the transaction was applied.
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// Optional external or internal reference identifier.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Optional description provided by the caller.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// UTC timestamp when the transaction was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
