using PaySphere.BuildingBlocks.Base;
using PaySphere.BuildingBlocks.Enums;

namespace PaySphere.WalletService.Entities;

public class Wallet : BaseEntity
{
    public int UserId { get; set; }

    public decimal Balance { get; set; }

    public WalletStatus Status { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
