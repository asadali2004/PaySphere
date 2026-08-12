using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.WalletService.Exceptions;

public class ReceiverNotFoundException : BaseException
{
    public ReceiverNotFoundException()
        : base("Receiver not found.")
    {
    }
}
