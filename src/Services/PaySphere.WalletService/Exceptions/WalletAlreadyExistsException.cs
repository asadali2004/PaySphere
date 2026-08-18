using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

/// <summary>
/// Thrown when an attempt is made to create a wallet for a user who already has one.
/// </summary>
public class WalletAlreadyExistsException : BaseException
{
    public WalletAlreadyExistsException()
        : base("Wallet already exists.")
    {
    }
}
