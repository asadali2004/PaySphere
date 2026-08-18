namespace PaySphere.WalletService.DTOs.Requests;

/// <summary>
/// Request model for withdrawing funds from a wallet.
/// </summary>
public class WithdrawRequest
{
    /// <summary>
    /// Amount to debit from the wallet. Must be positive and validated against balance.
    /// </summary>
    public decimal Amount { get; set; }
}
