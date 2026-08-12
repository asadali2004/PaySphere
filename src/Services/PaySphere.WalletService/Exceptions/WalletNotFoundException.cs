using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

public class WalletNotFoundException : BaseException
{
    public WalletNotFoundException()
        : base("Wallet not found.")
    {
    }
}
