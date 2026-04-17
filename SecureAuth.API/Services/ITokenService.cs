using System.Security.Claims;

namespace SecureAuth.API.Services;

/// <summary>
/// Interface for JWT token generation and management.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user with their roles.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="roles">The user's assigned roles.</param>
    /// <returns>A signed JWT token string.</returns>
    string GenerateToken(string userId, string email, IList<string> roles);

    /// <summary>
    /// Gets the expiration time for newly generated tokens.
    /// </summary>
    /// <returns>The expiration DateTime in UTC.</returns>
    DateTime GetExpirationTime();
}
