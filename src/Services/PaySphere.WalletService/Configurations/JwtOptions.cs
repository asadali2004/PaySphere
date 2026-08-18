namespace PaySphere.WalletService.Configurations
{
    /// <summary>
    /// Represents the configuration options for JWT (JSON Web Token) authentication.
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Gets the name of the configuration section for JWT options.
        /// </summary>
        public const string SectionName = "Jwt";

        /// <summary>
        /// Gets or sets the secret key used for signing and validating JWT tokens.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the issuer of the JWT tokens.
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the audience for which the JWT tokens are intended.
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expiry time for the JWT tokens in minutes.
        /// </summary>
        public int ExpiryInMinutes { get; set; }
    }
}
