namespace PaySphere.WalletService.Configurations;

/// <summary>
/// Represents the configuration options for the AuthService in the application.
/// </summary>
public class AuthServiceOptions
{
    /// <summary>
    /// Gets the name of the configuration section for AuthService options.
    /// </summary>
    public const string SectionName = "AuthService";

    /// <summary>
    /// Gets or sets the base URL of the AuthService, which is used for authentication and authorization purposes.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
}
