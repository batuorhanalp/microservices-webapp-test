using System.Security.Cryptography;
using WebApp.Common.DTOs;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
using WebApp.AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApp.AuthService.Services;

public class AuthService : IAuthService
{
    private readonly AuthUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AuthUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IUserSessionRepository userSessionRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _passwordResetTokenRepository = passwordResetTokenRepository ?? throw new ArgumentNullException(nameof(passwordResetTokenRepository));
        _userSessionRepository = userSessionRepository ?? throw new ArgumentNullException(nameof(userSessionRepository));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registering new user with email {Email}", request.Email);

        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request.Email));
        
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("Username is required", nameof(request.Username));
            
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required", nameof(request.Password));

        // Check if email is already taken
        if (await _userRepository.IsEmailTakenAsync(request.Email))
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            throw new ArgumentException("Email address is already registered", nameof(request.Email));
        }

        // Check if username is already taken
        if (await _userRepository.IsUsernameTakenAsync(request.Username))
        {
            _logger.LogWarning("Registration failed: Username {Username} already exists", request.Username);
            throw new ArgumentException("Username is already taken", nameof(request.Username));
        }

        // Hash password
        var passwordHash = _passwordService.HashPassword(request.Password);
        
        // Create user
        var user = new User(request.Email, request.Username, request.DisplayName, passwordHash);
        
        // Set optional fields
        if (request.BirthDate.HasValue)
            user.SetBirthDate(request.BirthDate.Value);
            
        if (!string.IsNullOrWhiteSpace(request.Bio))
            user.UpdateProfile(request.DisplayName, request.Bio, request.Website ?? "", request.Location ?? "");

        // Generate email confirmation token
        user.GenerateEmailConfirmationToken();

        // Save user
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User {UserId} registered successfully", user.Id);

        // Send confirmation email
        var confirmationLink = $"https://webapp.com/confirm-email?userId={user.Id}&token={user.EmailConfirmationToken}";
        await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink, user.DisplayName);

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(user.Email, user.DisplayName);

        // Generate tokens and session
        var sessionId = Guid.NewGuid().ToString();
        var accessToken = _jwtService.GenerateAccessToken(user, sessionId);
        var refreshToken = _jwtService.GenerateRefreshToken(user, sessionId, "0.0.0.0", "Registration");

        // Save refresh token
        await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

        // Create user session
        var userSession = new UserSession(
            user.Id,
            sessionId,
            "0.0.0.0",
            "Registration",
            DateTime.UtcNow.AddDays(7) // Session expires in 7 days
        );
        
        await _userSessionRepository.CreateAsync(userSession, cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), // Access token expiration
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for {EmailOrUsername}", request.EmailOrUsername);

        if (string.IsNullOrWhiteSpace(request.EmailOrUsername))
            throw new ArgumentException("Email or username is required", nameof(request.EmailOrUsername));
            
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required", nameof(request.Password));

        // Find user by email or username
        User? user = null;
        if (request.EmailOrUsername.Contains("@"))
        {
            user = await _userRepository.GetByEmailAsync(request.EmailOrUsername);
        }
        else
        {
            user = await _userRepository.GetByUsernameAsync(request.EmailOrUsername);
        }

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for {EmailOrUsername}", request.EmailOrUsername);
            throw new UnauthorizedAccessException("Invalid email/username or password");
        }

        // Check if account is locked
        if (user.IsLockedOut())
        {
            _logger.LogWarning("Login failed: Account locked for user {UserId}", user.Id);
            throw new UnauthorizedAccessException("Account is temporarily locked due to multiple failed attempts");
        }

        // Verify password
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
            
            // Record failed login attempt
            user.RecordFailedLoginAttempt();
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
            
            throw new UnauthorizedAccessException("Invalid email/username or password");
        }

        // Check if email is confirmed
        if (!user.IsEmailConfirmed)
        {
            _logger.LogWarning("Login failed: Email not confirmed for user {UserId}", user.Id);
            throw new UnauthorizedAccessException("Please confirm your email address before logging in");
        }

        // Successful login - reset failed attempts and update login time
        user.RecordSuccessfulLogin();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        // Generate session and tokens
        var sessionId = Guid.NewGuid().ToString();
        var accessToken = _jwtService.GenerateAccessToken(user, sessionId);
        var refreshToken = _jwtService.GenerateRefreshToken(user, sessionId, "0.0.0.0", "Login");

        // Save refresh token
        await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

        // Create user session
        var sessionExpiration = request.RememberMe 
            ? DateTime.UtcNow.AddDays(30) 
            : DateTime.UtcNow.AddDays(1);
            
        var userSession = new UserSession(
            user.Id,
            sessionId,
            "0.0.0.0",
            "Login",
            sessionExpiration
        );
        
        await _userSessionRepository.CreateAsync(userSession, cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing token");

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Refresh token is required", nameof(request.RefreshToken));

        // Find refresh token
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token not found");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if token is active
        if (!refreshToken.IsActive())
        {
            _logger.LogWarning("Refresh token {TokenId} is not active", refreshToken.Id);
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for refresh token", refreshToken.UserId);
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Generate new tokens
        var newSessionId = Guid.NewGuid().ToString();
        var newAccessToken = _jwtService.GenerateAccessToken(user, newSessionId);
        var newRefreshToken = _jwtService.GenerateRefreshToken(user, newSessionId, "0.0.0.0", "Token Refresh");

        // Revoke old token
        _jwtService.RevokeToken(refreshToken, "0.0.0.0", "Replaced by new token", newRefreshToken.Token);
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

        // Save new token
        await _refreshTokenRepository.CreateAsync(newRefreshToken, cancellationToken);

        _logger.LogDebug("Token refreshed successfully for user {UserId}", user.Id);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserDto(user)
        };
    }

    public async Task LogoutAsync(Guid userId, string? refreshToken = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Logging out user {UserId}", userId);

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
            if (token != null && token.UserId == userId)
            {
                _jwtService.RevokeToken(token, "0.0.0.0", "User logout");
                await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
            }
        }

        _logger.LogInformation("User {UserId} logged out successfully", userId);
    }

    public async Task LogoutAllDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Logging out user {UserId} from all devices", userId);

        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, "0.0.0.0", "Logout all devices", cancellationToken);
        await _userSessionRepository.DeactivateAllUserSessionsAsync(userId, cancellationToken);

        _logger.LogInformation("User {UserId} logged out from all devices", userId);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Changing password for user {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect");

        var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.ChangePassword(newPasswordHash);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        // Revoke all refresh tokens to force re-login
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, "0.0.0.0", "Password changed", cancellationToken);

        // Send notification email
        await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.DisplayName);

        _logger.LogInformation("Password changed successfully for user {UserId}", userId);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Password reset requested for email {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal if email exists or not
            _logger.LogWarning("Password reset requested for non-existent email {Email}", request.Email);
            return;
        }

        // Invalidate existing password reset tokens
        await _passwordResetTokenRepository.InvalidateAllUserTokensAsync(user.Id, cancellationToken);

        // Generate new reset token
        var resetToken = new PasswordResetToken(
            user.Id,
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
            "0.0.0.0"
        );

        await _passwordResetTokenRepository.CreateAsync(resetToken, cancellationToken);

        // Send reset email
        var resetLink = $"https://webapp.com/reset-password?token={resetToken.Token}&email={request.Email}";
        await _emailService.SendPasswordResetAsync(user.Email, resetLink, user.DisplayName);

        _logger.LogInformation("Password reset email sent for user {UserId}", user.Id);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resetting password for email {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new ArgumentException("Invalid reset token", nameof(request.Token));

        var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(request.Token, cancellationToken);
        if (resetToken == null || resetToken.UserId != user.Id || resetToken.IsExpired() || resetToken.IsUsed)
            throw new ArgumentException("Invalid or expired reset token", nameof(request.Token));

        // Reset password
        var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.ChangePassword(newPasswordHash);
        
        // Mark token as used
        resetToken.MarkAsUsed();

        _userRepository.Update(user);
        await _passwordResetTokenRepository.UpdateAsync(resetToken, cancellationToken);
        await _userRepository.SaveChangesAsync();

        // Revoke all refresh tokens
        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, "0.0.0.0", "Password reset", cancellationToken);

        // Send confirmation email
        await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.DisplayName);

        _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(ValidateTokenRequest request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtService.ValidateToken(request.Token);
        if (principal == null)
        {
            return new TokenValidationResponse
            {
                IsValid = false,
                ErrorMessage = "Invalid token"
            };
        }

        var userId = principal.Claims.FirstOrDefault(x => x.Type == JwtClaims.UserId)?.Value;
        var username = principal.Claims.FirstOrDefault(x => x.Type == JwtClaims.Username)?.Value;
        var email = principal.Claims.FirstOrDefault(x => x.Type == JwtClaims.Email)?.Value;
        var expiration = _jwtService.GetTokenExpiration(request.Token);

        return new TokenValidationResponse
        {
            IsValid = true,
            UserId = userId,
            Username = username,
            Email = email,
            ExpiresAt = expiration
        };
    }

    public async Task ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            throw new ArgumentException("Invalid user ID", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userGuid);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        if (user.EmailConfirmationToken != token)
            throw new ArgumentException("Invalid confirmation token", nameof(token));

        user.ConfirmEmail();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Email confirmed for user {UserId}", user.Id);
    }

    public async Task ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || user.IsEmailConfirmed)
            return; // Don't reveal if email exists

        user.GenerateEmailConfirmationToken();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        var confirmationLink = $"https://webapp.com/confirm-email?userId={user.Id}&token={user.EmailConfirmationToken}";
        await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink, user.DisplayName);

        _logger.LogInformation("Email confirmation resent for user {UserId}", user.Id);
    }

    // Simplified 2FA methods (can be enhanced with proper TOTP later)
    public async Task<string> EnableTwoFactorAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.EnableTwoFactorAuthentication(secret);
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("2FA enabled for user {UserId}", userId);
        return secret;
    }

    public async Task DisableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        // Simple validation - in production, use proper TOTP validation
        if (code != "123456") // Placeholder
            throw new UnauthorizedAccessException("Invalid 2FA code");

        user.DisableTwoFactorAuthentication();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("2FA disabled for user {UserId}", userId);
    }

    public async Task<bool> VerifyTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        // Placeholder implementation - use proper TOTP in production
        return code == "123456";
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _userSessionRepository.GetActiveSessionsByUserIdAsync(userId, cancellationToken);
    }

    public async Task RevokeSessionAsync(Guid userId, string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _userSessionRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (session == null || session.UserId != userId)
            throw new ArgumentException("Session not found", nameof(sessionId));

        await _userSessionRepository.DeactivateSessionAsync(sessionId, cancellationToken);
        
        _logger.LogInformation("Session {SessionId} revoked for user {UserId}", sessionId, userId);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl,
            CoverImageUrl = user.CoverImageUrl,
            Website = user.Website,
            Location = user.Location,
            BirthDate = user.BirthDate,
            IsPrivate = user.IsPrivate,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
