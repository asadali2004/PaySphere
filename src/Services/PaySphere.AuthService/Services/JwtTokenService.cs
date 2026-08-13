using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PaySphere.AuthService.Configurations;
using PaySphere.AuthService.Entities;
using PaySphere.AuthService.Services.Interfaces;

namespace PaySphere.AuthService.Services;

internal class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Generates a signed JWT containing minimal claims required by other services:
    /// - NameIdentifier contains the authenticated user's Id so services can authorize actions.
    /// - Name and Email are included for convenience in logs or simple UI displays.
    /// - Role claim is optional and used for authorization policies.
    /// The token is signed with a symmetric key configured in JwtOptions.
    /// </summary>
    public (string Token, DateTime ExpiresAt) GenerateToken(User user, string roleName)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryInMinutes);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
        };

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        return (tokenString, expiresAt);
    }
}
