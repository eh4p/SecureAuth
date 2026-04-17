using System.ComponentModel.DataAnnotations;

namespace SecureAuth.API.Models.DTOs;

/// <summary>
/// Request model for user registration.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password.
    /// </summary>
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// User's full name.
    /// </summary>
    [Required]
    public string FullName { get; set; } = string.Empty;
}
