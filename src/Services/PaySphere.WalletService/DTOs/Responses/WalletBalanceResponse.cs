namespace PaySphere.WalletService.DTOs.Responses;

/// <summary>
/// Response for wallet balance queries returning the current decimal balance.
/// </summary>
public class WalletBalanceResponse
{
    /// <summary>
    /// Current wallet balance.
    /// </summary>
    public decimal Balance { get; set; }
}
