using PaySphere.WalletService.DTOs.Responses;

namespace PaySphere.WalletService.Services.Interfaces;

public interface IAuthServiceClient
{
    /// <summary>
    /// Calls the AuthService to validate a receiver user id. Returns a minimal
    /// validation response indicating existence and active state.
    /// </summary>
    Task<ReceiverValidationResponse> ValidateReceiverAsync(int userId);
}
