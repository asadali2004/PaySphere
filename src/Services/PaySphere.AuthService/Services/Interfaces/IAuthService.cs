using PaySphere.AuthService.DTOs.Requests;
using PaySphere.AuthService.DTOs.Responses;

namespace PaySphere.AuthService.Services.Interfaces;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request);

    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<UserResponse> GetProfileAsync(int userId);

    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
