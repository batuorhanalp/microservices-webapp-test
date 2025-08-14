using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Common.DTOs;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> RegisterAsync([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetProfile), new { }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Registration failed with argument error");
            return BadRequest(new AuthErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthErrorResponse
            {
                Error = "server_error",
                ErrorDescription = "An unexpected error occurred during registration",
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Login with email/username and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Login failed with argument error");
            return BadRequest(new AuthErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed with unauthorized error");
            return Unauthorized(new AuthErrorResponse
            {
                Error = "invalid_grant",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status401Unauthorized
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthErrorResponse
            {
                Error = "server_error",
                ErrorDescription = "An unexpected error occurred during login",
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Token refresh failed with argument error");
            return BadRequest(new AuthErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Token refresh failed with unauthorized error");
            return Unauthorized(new AuthErrorResponse
            {
                Error = "invalid_grant",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status401Unauthorized
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthErrorResponse
            {
                Error = "server_error",
                ErrorDescription = "An unexpected error occurred during token refresh",
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Logout current session
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync([FromBody] RefreshTokenRequest? request = null)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            await _authService.LogoutAsync(userId, request?.RefreshToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Logout from all devices
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAllDevicesAsync()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            await _authService.LogoutAllDevicesAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout all devices failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserIdFromClaims();
            await _authService.ChangePasswordAsync(userId, request);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Change password failed with argument error");
            return BadRequest(new AuthErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Change password failed with unauthorized error");
            return Unauthorized(new AuthErrorResponse
            {
                Error = "invalid_grant",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status401Unauthorized
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change password failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Request password reset email
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _authService.ForgotPasswordAsync(request);
            return NoContent(); // Always return success to prevent email enumeration
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Forgot password failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Reset password using reset token
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _authService.ResetPasswordAsync(request);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Reset password failed with argument error");
            return BadRequest(new AuthErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reset password failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    [HttpPost("validate-token")]
    [ProducesResponseType(typeof(TokenValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenValidationResponse>> ValidateTokenAsync([FromBody] ValidateTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.ValidateTokenAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new AuthErrorResponse
                {
                    Error = "invalid_request",
                    ErrorDescription = "User ID and token are required",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            await _authService.ConfirmEmailAsync(userId, token);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Email confirmation failed with argument error");
            return BadRequest(new AuthErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email confirmation failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Resend email confirmation
    /// </summary>
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailConfirmationAsync([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _authService.ResendEmailConfirmationAsync(request.Email);
            return NoContent(); // Always return success to prevent email enumeration
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resend email confirmation failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public ActionResult<UserDto> GetProfile()
    {
        // This would typically fetch the full user profile from the database
        // For now, return basic info from JWT claims
        var userDto = new UserDto
        {
            Id = GetUserIdFromClaims(),
            Email = User.FindFirst(JwtClaims.Email)?.Value ?? "",
            Username = User.FindFirst(JwtClaims.Username)?.Value ?? "",
            DisplayName = User.FindFirst(JwtClaims.DisplayName)?.Value ?? "",
            IsVerified = bool.Parse(User.FindFirst(JwtClaims.IsVerified)?.Value ?? "false")
        };

        return Ok(userDto);
    }

    /// <summary>
    /// Enable two-factor authentication
    /// </summary>
    [HttpPost("2fa/enable")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> EnableTwoFactorAsync()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var secret = await _authService.EnableTwoFactorAsync(userId);
            return Ok(new { secret });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Enable 2FA failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    [HttpPost("2fa/disable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DisableTwoFactorAsync([FromBody] TwoFactorCodeRequest request)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            await _authService.DisableTwoFactorAsync(userId, request.Code);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Disable 2FA failed with unauthorized error");
            return Unauthorized(new AuthErrorResponse
            {
                Error = "invalid_code",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status401Unauthorized
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Disable 2FA failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get active user sessions
    /// </summary>
    [HttpGet("sessions")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<UserSession>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserSession>>> GetActiveSessionsAsync()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var sessions = await _authService.GetActiveSessionsAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get active sessions failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeSessionAsync(string sessionId)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            await _authService.RevokeSessionAsync(userId, sessionId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Revoke session failed with argument error");
            return BadRequest(new AuthErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Revoke session failed with unexpected error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(JwtClaims.UserId)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}

// Additional DTOs for controller-specific requests
public class TwoFactorCodeRequest
{
    public string Code { get; set; } = string.Empty;
}
