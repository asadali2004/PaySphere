using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

public class WalletNotActiveException : BaseException
{
    public WalletNotActiveException()
        : base("Wallet is not active.")
    {
    }
}
