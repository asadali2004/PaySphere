using PaySphere.BuildingBlocks.Enums;

namespace PaySphere.WalletService.DTOs.Responses;

/// <summary>
/// Public wallet representation returned by the WalletService APIs. Contains
/// non-sensitive wallet fields for the authenticated user.
/// </summary>
public class WalletResponse
{
    /// <summary>
    /// Wallet primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Owner user id.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Current wallet balance using decimal currency semantics.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Current wallet status (Active, Suspended, etc.).
    /// </summary>
    public WalletStatus Status { get; set; }

    /// <summary>
    /// UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC last update timestamp when available.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
