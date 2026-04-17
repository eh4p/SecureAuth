namespace SecureAuth.API.Models.DTOs;

/// <summary>
/// Response model for successful authentication.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT token for authorization.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's assigned roles.
    /// </summary>
    public IList<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Token expiration timestamp.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
