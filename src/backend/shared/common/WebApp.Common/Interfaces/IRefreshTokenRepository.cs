using WebApp.Common.Entities;

namespace WebApp.Common.Interfaces;

public interface IRefreshTokenRepository
{
    /// <summary>
    /// Get refresh token by token string
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all active refresh tokens for a user
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add new refresh token
    /// </summary>
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update refresh token
    /// </summary>
    Task<RefreshToken> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete refresh token
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revoke all tokens for a user
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, string ipAddress, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clean up expired tokens
    /// </summary>
    Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get refresh token usage statistics
    /// </summary>
    Task<Dictionary<string, int>> GetTokenUsageStatsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IPasswordResetTokenRepository
{
    /// <summary>
    /// Get password reset token by token string
    /// </summary>
    Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get active password reset token for user
    /// </summary>
    Task<PasswordResetToken?> GetActiveTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create new password reset token
    /// </summary>
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update password reset token
    /// </summary>
    Task<PasswordResetToken> UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete password reset token
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidate all tokens for user
    /// </summary>
    Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clean up expired tokens
    /// </summary>
    Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}

public interface IUserSessionRepository
{
    /// <summary>
    /// Get session by session ID
    /// </summary>
    Task<UserSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all active sessions for a user
    /// </summary>
    Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create new user session
    /// </summary>
    Task<UserSession> CreateAsync(UserSession session, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update user session
    /// </summary>
    Task<UserSession> UpdateAsync(UserSession session, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deactivate session
    /// </summary>
    Task DeactivateSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deactivate all user sessions
    /// </summary>
    Task DeactivateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clean up expired sessions
    /// </summary>
    Task<int> DeleteExpiredSessionsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update session activity
    /// </summary>
    Task UpdateActivityAsync(string sessionId, CancellationToken cancellationToken = default);
}
