namespace PaySphere.WalletService.Configurations;

public class AuthServiceOptions
{
    public const string SectionName = "AuthService";

    public string BaseUrl { get; set; } = string.Empty;
}
