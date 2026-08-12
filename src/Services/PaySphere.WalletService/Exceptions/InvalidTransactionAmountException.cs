using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

public class InvalidTransactionAmountException : BaseException
{
    public InvalidTransactionAmountException()
        : base("Invalid transaction amount.")
    {
    }
}
