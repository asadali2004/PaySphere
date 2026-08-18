using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

/// <summary>
/// Thrown when a transfer's target receiver cannot be validated or does not exist.
/// Downstream services should use this to return a clear client error.
/// </summary>
public class ReceiverNotFoundException : BaseException
{
    public ReceiverNotFoundException()
        : base("Receiver not found.")
    {
    }
}
