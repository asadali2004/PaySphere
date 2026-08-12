using PaySphere.WalletService.DTOs.Responses;

namespace PaySphere.WalletService.Services.Interfaces;

public interface IAuthServiceClient
{
    Task<ReceiverValidationResponse> ValidateReceiverAsync(int userId);
}
