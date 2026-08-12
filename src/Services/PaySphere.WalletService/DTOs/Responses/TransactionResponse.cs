using PaySphere.BuildingBlocks.Enums;

namespace PaySphere.WalletService.DTOs.Responses;

public class TransactionResponse
{
    public int Id { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Reference { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}
