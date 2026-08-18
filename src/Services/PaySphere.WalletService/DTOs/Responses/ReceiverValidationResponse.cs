namespace PaySphere.WalletService.DTOs.Responses;

/// <summary>
/// Response returned by AuthService client when validating a receiver prior to transfer.
/// Mirrors the minimal validation contract used across services.
/// </summary>
public class ReceiverValidationResponse
{
    /// <summary>
    /// The validated user id.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// True when the user exists.
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// True when the user's account is active.
    /// </summary>
    public bool IsActive { get; set; }
}
