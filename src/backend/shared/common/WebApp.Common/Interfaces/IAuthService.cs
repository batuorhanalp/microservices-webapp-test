using WebApp.Common.DTOs;
using WebApp.Common.Entities;

namespace WebApp.Common.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Authenticate user and return tokens
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Logout user and revoke tokens
    /// </summary>
    Task LogoutAsync(Guid userId, string? refreshToken = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Logout user from all devices
    /// </summary>
    Task LogoutAllDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Change user password
    /// </summary>
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send password reset email
    /// </summary>
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reset password using token
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate JWT token
    /// </summary>
    Task<TokenValidationResponse> ValidateTokenAsync(ValidateTokenRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Confirm email address
    /// </summary>
    Task ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resend email confirmation
    /// </summary>
    Task ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enable two-factor authentication
    /// </summary>
    Task<string> EnableTwoFactorAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    Task DisableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verify two-factor authentication code
    /// </summary>
    Task<bool> VerifyTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user active sessions
    /// </summary>
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revoke specific session
    /// </summary>
    Task RevokeSessionAsync(Guid userId, string sessionId, CancellationToken cancellationToken = default);
}
