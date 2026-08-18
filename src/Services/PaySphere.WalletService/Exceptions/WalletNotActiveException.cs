using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

/// <summary>
/// Thrown when an operation is attempted on a wallet that is not in an active state.
/// </summary>
public class WalletNotActiveException : BaseException
{
    public WalletNotActiveException()
        : base("Wallet is not active.")
    {
    }
}
