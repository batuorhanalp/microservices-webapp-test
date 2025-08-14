using System.Security.Claims;
using WebApp.Common.DTOs;
using WebApp.Common.Entities;

namespace WebApp.Common.Interfaces;

public interface IJwtService
{
    /// <summary>
    /// Generate JWT access token for user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>JWT token string</returns>
    string GenerateAccessToken(User user, string sessionId);
    
    /// <summary>
    /// Generate refresh token
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="userAgent">Client user agent</param>
    /// <returns>Refresh token entity</returns>
    RefreshToken GenerateRefreshToken(User user, string sessionId, string ipAddress, string userAgent);
    
    /// <summary>
    /// Validate JWT token and extract claims
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>Claims principal if valid, null otherwise</returns>
    ClaimsPrincipal? ValidateToken(string token);
    
    /// <summary>
    /// Get token expiration time
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>Expiration date time</returns>
    DateTime? GetTokenExpiration(string token);
    
    /// <summary>
    /// Extract user ID from token
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>User ID if valid</returns>
    Guid? GetUserIdFromToken(string token);
    
    /// <summary>
    /// Extract session ID from token
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>Session ID if valid</returns>
    string? GetSessionIdFromToken(string token);
    
    /// <summary>
    /// Check if token is expired
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>True if expired</returns>
    bool IsTokenExpired(string token);
    
    /// <summary>
    /// Revoke refresh token
    /// </summary>
    /// <param name="token">Refresh token to revoke</param>
    /// <param name="ipAddress">IP address initiating revocation</param>
    /// <param name="reason">Reason for revocation</param>
    /// <param name="replacedByToken">New token that replaces this one</param>
    void RevokeToken(RefreshToken token, string ipAddress, string? reason = null, string? replacedByToken = null);
}
