using PaySphere.WalletService.DTOs.Requests;
using PaySphere.WalletService.Exceptions;

namespace PaySphere.WalletService.Validators;

/// <summary>
/// Validates TopUpRequest payloads and throws <see cref="PaySphere.WalletService.Exceptions.InvalidTransactionAmountException"/>
/// for invalid amounts. Amounts must be positive and have at most two decimal places.
/// </summary>
internal static class TopUpRequestValidator
{
    public static void Validate(TopUpRequest request)
    {
        ValidateAmount(request?.Amount ?? 0);
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
        {
            throw new InvalidTransactionAmountException();
        }
    }
}
