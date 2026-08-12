using PaySphere.WalletService.DTOs.Requests;
using PaySphere.WalletService.Exceptions;

namespace PaySphere.WalletService.Validators;

internal static class WithdrawRequestValidator
{
    public static void Validate(WithdrawRequest request)
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
