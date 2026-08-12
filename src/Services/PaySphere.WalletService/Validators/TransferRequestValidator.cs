using PaySphere.BuildingBlocks.Exceptions;
using PaySphere.WalletService.DTOs.Requests;
using PaySphere.WalletService.Exceptions;

namespace PaySphere.WalletService.Validators;

internal static class TransferRequestValidator
{
    public static void Validate(TransferRequest request)
    {
        if (request is null)
        {
            throw new BaseException("Request cannot be null.");
        }

        if (request.ReceiverUserId <= 0)
        {
            throw new BaseException("ReceiverUserId must be greater than zero.");
        }

        if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
        {
            throw new BaseException("Description must not exceed 500 characters.");
        }

        if (request.Amount <= 0 || decimal.Round(request.Amount, 2) != request.Amount)
        {
            throw new InvalidTransactionAmountException();
        }
    }
}
