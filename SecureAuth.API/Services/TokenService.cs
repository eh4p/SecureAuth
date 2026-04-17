using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SecureAuth.API.Services;

/// <summary>
/// Service for generating JWT tokens.
/// NOTE: Uses fake/hardcoded secret key for demonstration purposes only.
/// </summary>
public class TokenService : ITokenService
{
    private const string FakeSecretKey = "SecureAuth_FakeSecretKey_32Chars!!";
    private const string Issuer = "SecureAuth.API";
    private const string Audience = "SecureAuth.API";
    private const int TokenExpirationMinutes = 60;

    private readonly SymmetricSecurityKey _signingKey;
    private readonly ILogger<TokenService> _logger;

    public TokenService(ILogger<TokenService> logger)
    {
        _logger = logger;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(FakeSecretKey));
    }

    /// <summary>
    /// Generates a JWT token containing user claims and roles.
    /// </summary>
    public string GenerateToken(string userId, string email, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var expires = GetExpirationTime();

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        _logger.LogInformation("Generated JWT token for user {Email} with roles: {Roles}", 
            email, string.Join(", ", roles));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Returns token expiration time (60 minutes from now).
    /// </summary>
    public DateTime GetExpirationTime()
    {
        return DateTime.UtcNow.AddMinutes(TokenExpirationMinutes);
    }

    /// <summary>
    /// Returns the signing key for JWT validation configuration.
    /// </summary>
    public static SymmetricSecurityKey GetSigningKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(FakeSecretKey));
    }

    /// <summary>
    /// Returns the token issuer.
    /// </summary>
    public static string GetIssuer() => Issuer;

    /// <summary>
    /// Returns the token audience.
    /// </summary>
    public static string GetAudience() => Audience;
}
