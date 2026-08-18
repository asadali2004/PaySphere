using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

/// <summary>
/// Thrown when the requested wallet cannot be located for a given user or id.
/// </summary>
public class WalletNotFoundException : BaseException
{
    public WalletNotFoundException()
        : base("Wallet not found.")
    {
    }
}
