using System.Net.Http.Json;
using System.Text.Json;
using PaySphere.BuildingBlocks.Responses;
using PaySphere.WalletService.DTOs.Responses;
using PaySphere.WalletService.Services.Interfaces;

namespace PaySphere.WalletService.Services;

/// <summary>
/// HTTP client used to call AuthService for lightweight receiver validation.
/// WalletService delegates user-related validation to AuthService to keep services decoupled and authoritative.
/// </summary>
public class AuthServiceClient : IAuthServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public AuthServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReceiverValidationResponse> ValidateReceiverAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"api/v1/internal/users/{userId}");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ReceiverValidationResponse>>(JsonOptions);

        if (payload?.Data is null)
        {
            throw new InvalidOperationException("Receiver validation response was empty.");
        }

        return payload.Data;
    }
}
