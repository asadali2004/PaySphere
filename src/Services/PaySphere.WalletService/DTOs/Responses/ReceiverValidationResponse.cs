namespace PaySphere.WalletService.DTOs.Responses;

public class ReceiverValidationResponse
{
    public int UserId { get; set; }

    public bool Exists { get; set; }

    public bool IsActive { get; set; }
}
