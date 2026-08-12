using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

public class WalletAlreadyExistsException : BaseException
{
    public WalletAlreadyExistsException()
        : base("Wallet already exists.")
    {
    }
}
