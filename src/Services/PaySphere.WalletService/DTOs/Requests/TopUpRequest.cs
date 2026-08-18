namespace PaySphere.WalletService.DTOs.Requests;

/// <summary>
/// Request model for topping up a wallet. Amount is a decimal with two
/// fractional digits representing currency units.
/// </summary>
public class TopUpRequest
{
    /// <summary>
    /// Amount to credit to the wallet. Must be a positive decimal with 2 decimal places.
    /// </summary>
    public decimal Amount { get; set; }
}
