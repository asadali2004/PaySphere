using PaySphere.BuildingBlocks.Enums;

namespace PaySphere.WalletService.DTOs.Responses;

public class WalletResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal Balance { get; set; }

    public WalletStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
