namespace PaySphere.WalletService.DTOs.Requests;

public class TransferRequest
{
    public int ReceiverUserId { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }
}
