using System.ComponentModel.DataAnnotations;

namespace WebApp.Common.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    
    [Required]
    public Guid UserId { get; private set; }
    
    [Required]
    public string Token { get; private set; } = string.Empty;
    
    public string JwtId { get; private set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    
    public bool IsUsed { get; private set; }
    
    public bool IsRevoked { get; private set; }
    
    public DateTime? RevokedAt { get; private set; }
    
    public string? RevokedByIp { get; private set; }
    
    public string? RevokedReason { get; private set; }
    
    public string? ReplacedByToken { get; private set; }
    
    public string? IpAddress { get; private set; }
    
    public string? UserAgent { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    // Constructors
    private RefreshToken() { } // For EF Core
    
    public RefreshToken(
        Guid userId, 
        string token, 
        string jwtId,
        DateTime expiresAt,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
            
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required", nameof(token));
            
        if (string.IsNullOrWhiteSpace(jwtId))
            throw new ArgumentException("JWT ID is required", nameof(jwtId));
            
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiry date must be in the future", nameof(expiresAt));

        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        JwtId = jwtId;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IsUsed = false;
        IsRevoked = false;
    }

    // Domain methods
    public bool IsActive => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiresAt;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Refresh token has already been used");
            
        if (IsRevoked)
            throw new InvalidOperationException("Cannot use a revoked refresh token");
            
        if (IsExpired)
            throw new InvalidOperationException("Cannot use an expired refresh token");
            
        IsUsed = true;
    }
    
    public void Revoke(string? ipAddress = null, string? reason = null, string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        RevokedReason = reason;
        ReplacedByToken = replacedByToken;
    }
    
    public void RevokeDescendantRefreshTokens(string? ipAddress = null, string? reason = null)
    {
        // This method would be used in conjunction with a token family concept
        // to revoke all tokens in a refresh token chain if compromise is detected
        Revoke(ipAddress, reason ?? "Compromised token chain detected");
    }
}

public class PasswordResetToken
{
    public Guid Id { get; private set; }
    
    [Required]
    public Guid UserId { get; private set; }
    
    [Required]
    public string Token { get; private set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    
    public bool IsUsed { get; private set; }
    
    public DateTime? UsedAt { get; private set; }
    
    public string? IpAddress { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    // Constructors
    private PasswordResetToken() { } // For EF Core
    
    public PasswordResetToken(
        Guid userId,
        string token,
        DateTime expiresAt,
        string? ipAddress = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
            
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required", nameof(token));
            
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiry date must be in the future", nameof(expiresAt));

        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        IsUsed = false;
    }

    // Domain methods
    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Password reset token has already been used");
            
        if (IsExpired)
            throw new InvalidOperationException("Password reset token has expired");
            
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}

public class UserSession
{
    public Guid Id { get; private set; }
    
    [Required]
    public Guid UserId { get; private set; }
    
    public string SessionId { get; private set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime LastActivityAt { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    
    public string? IpAddress { get; private set; }
    
    public string? UserAgent { get; private set; }
    
    public string? DeviceInfo { get; private set; }
    
    public string? Location { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    // Constructors
    private UserSession() { } // For EF Core
    
    public UserSession(
        Guid userId,
        string sessionId,
        DateTime expiresAt,
        string? ipAddress = null,
        string? userAgent = null,
        string? deviceInfo = null,
        string? location = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
            
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID is required", nameof(sessionId));

        Id = Guid.NewGuid();
        UserId = userId;
        SessionId = sessionId;
        CreatedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        DeviceInfo = deviceInfo;
        Location = location;
        IsActive = true;
    }

    // Domain methods
    public void UpdateActivity()
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot update activity for inactive session");
            
        LastActivityAt = DateTime.UtcNow;
    }
    
    public void Terminate()
    {
        IsActive = false;
    }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public bool IsValidSession => IsActive && !IsExpired;
}
