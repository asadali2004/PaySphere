using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

/// <summary>
/// Thrown when a withdrawal or transfer would cause the wallet balance to go negative.
/// This is a business-level exception and will be translated to a 400 Bad Request
/// by the global exception middleware.
/// </summary>
public class InsufficientBalanceException : BaseException
{
    public InsufficientBalanceException()
        : base("Insufficient wallet balance.")
    {
    }
}
