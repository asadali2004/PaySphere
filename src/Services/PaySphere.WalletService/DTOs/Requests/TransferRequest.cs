namespace PaySphere.WalletService.DTOs.Requests;

/// <summary>
/// Request model describing a transfer from the authenticated user to another user.
/// The sender's identity is derived from the authenticated JWT; receiver is specified here.
/// </summary>
public class TransferRequest
{
    /// <summary>
    /// Target user id who will receive the funds.
    /// </summary>
    public int ReceiverUserId { get; set; }

    /// <summary>
    /// Amount to transfer. Must be positive and validated against sender balance.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Optional transfer description recorded with transaction history.
    /// </summary>
    public string? Description { get; set; }
}
