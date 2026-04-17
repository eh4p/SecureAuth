using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureAuth.API.Models.DTOs;
using SecureAuth.API.Services;

namespace SecureAuth.API.Controllers;

/// <summary>
/// Handles authentication operations including login, registration, and external OAuth.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    private const string DefaultUserRole = "Employee";

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>Auth response with JWT token.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            Roles = roles,
            ExpiresAt = _tokenService.GetExpirationTime()
        });
    }

    /// <summary>
    /// Registers a new user with Employee role by default.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <returns>Auth response with JWT token.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return BadRequest(new { Message = "Email already registered" });
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("User registration failed for {Email}: {Errors}", 
                request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(new { Message = "Registration failed", Errors = result.Errors });
        }

        await _userManager.AddToRoleAsync(user, DefaultUserRole);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

        _logger.LogInformation("User {Email} registered successfully", request.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            Roles = roles,
            ExpiresAt = _tokenService.GetExpirationTime()
        });
    }

    /// <summary>
    /// Returns the current authenticated user's information from JWT claims.
    /// </summary>
    /// <returns>User information.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            UserId = userId,
            Email = email,
            Roles = roles
        });
    }

    /// <summary>
    /// Initiates external OAuth login flow (e.g., Google).
    /// NOTE: Uses fake OAuth credentials for demonstration.
    /// </summary>
    /// <param name="provider">External login provider name (e.g., "Google").</param>
    [HttpGet("external-login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult ExternalLogin([FromQuery] string provider = "Google")
    {
        var redirectUrl = Url.Action(nameof(ExternalCallback), "Auth");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        
        _logger.LogInformation("Initiating external login with provider: {Provider}", provider);
        
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Handles the OAuth callback, creates/updates the user, and returns a JWT token.
    /// NOTE: Fake OAuth callback - returns error in demo mode.
    /// </summary>
    [HttpGet("external-callback")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExternalCallback()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            _logger.LogWarning("External login callback failed - no login info");
            return BadRequest(new { Message = "External login failed" });
        }

        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("External login callback failed - no email claim");
            return BadRequest(new { Message = "Email claim not found" });
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create user from external login: {Email}", email);
                return BadRequest(new { Message = "Failed to create user", Errors = result.Errors });
            }

            await _userManager.AddToRoleAsync(user, DefaultUserRole);
            _logger.LogInformation("Created new user from external login: {Email}", email);
        }

        if (info.LoginProvider is not null)
        {
            var logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any(l => l.LoginProvider == info.LoginProvider))
            {
                await _userManager.AddLoginAsync(user, info);
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

        _logger.LogInformation("External login successful for: {Email}", email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            Roles = roles,
            ExpiresAt = _tokenService.GetExpirationTime()
        });
    }
}
