using PaySphere.BuildingBlocks.Base;
using PaySphere.BuildingBlocks.Enums;

namespace PaySphere.WalletService.Entities;

/// <summary>
/// Represents a user's wallet. The service stores UserId rather than duplicating a Users table
/// to keep the WalletService autonomous and avoid cross-service transactional coupling.
/// Other services (e.g. AuthService) are queried when user validation is required.
/// </summary>
public class Wallet : BaseEntity
{
    // Corresponding user id from AuthService. This keeps the domain model simple and avoids
    // maintaining a duplicated user details table here.
    public int UserId { get; set; }

    // Current balance for the wallet. Decimal is used for currency values.
    public decimal Balance { get; set; }

    // Status indicates whether operations are allowed (Active) or blocked (Frozen/Closed).
    public WalletStatus Status { get; set; }

    // Transaction history navigation property. Kept as ICollection for EF Core relationship mapping.
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
