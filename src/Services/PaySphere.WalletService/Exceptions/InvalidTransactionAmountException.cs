using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

/// <summary>
/// Thrown when a provided transaction amount is zero or negative, or otherwise invalid
/// according to business validation rules.
/// </summary>
public class InvalidTransactionAmountException : BaseException
{
    public InvalidTransactionAmountException()
        : base("Invalid transaction amount.")
    {
    }
}
