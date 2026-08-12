using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

public class InsufficientBalanceException : BaseException
{
    public InsufficientBalanceException()
        : base("Insufficient wallet balance.")
    {
    }
}
