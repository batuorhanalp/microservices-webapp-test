using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApp.Common.DTOs;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.AuthService.Services;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtService> _logger;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
        };
    }

    public string GenerateAccessToken(User user, string sessionId)
    {
        var claims = new[]
        {
            new Claim(JwtClaims.UserId, user.Id.ToString()),
            new Claim(JwtClaims.Username, user.Username),
            new Claim(JwtClaims.Email, user.Email),
            new Claim(JwtClaims.DisplayName, user.DisplayName),
            new Claim(JwtClaims.IsVerified, user.IsVerified.ToString()),
            new Claim(JwtClaims.TokenType, "access"),
            new Claim(JwtClaims.SessionId, sessionId),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogDebug("Generated access token for user {UserId} with session {SessionId}", user.Id, sessionId);
        
        return tokenString;
    }

    public RefreshToken GenerateRefreshToken(User user, string sessionId, string ipAddress, string userAgent)
    {
        using var rngCryptoServiceProvider = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rngCryptoServiceProvider.GetBytes(randomBytes);
        var refreshTokenString = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenString,
            Guid.NewGuid().ToString(), // JTI
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            ipAddress,
            userAgent
        );

        _logger.LogDebug("Generated refresh token for user {UserId} with session {SessionId}", user.Id, sessionId);
        
        return refreshToken;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken validatedToken);
            
            // Ensure token is JWT
            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid JWT token algorithm");
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    public DateTime? GetTokenExpiration(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read token expiration");
            return null;
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
            return null;

        var userIdClaim = principal.Claims.FirstOrDefault(x => x.Type == JwtClaims.UserId);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    public string? GetSessionIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
            return null;

        return principal.Claims.FirstOrDefault(x => x.Type == JwtClaims.SessionId)?.Value;
    }

    public bool IsTokenExpired(string token)
    {
        var expiration = GetTokenExpiration(token);
        return expiration.HasValue && expiration.Value <= DateTime.UtcNow;
    }

    public void RevokeToken(RefreshToken token, string ipAddress, string? reason = null, string? replacedByToken = null)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        token.Revoke(ipAddress, reason, replacedByToken);
        
        _logger.LogInformation("Revoked refresh token {TokenId} for user {UserId}. Reason: {Reason}", 
            token.Id, token.UserId, reason ?? "Manual revocation");
    }
}
