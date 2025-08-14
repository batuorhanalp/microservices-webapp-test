using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.AuthService.Data;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        return refreshToken;
    }

    public async Task<RefreshToken> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        return refreshToken;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens.FindAsync(new object[] { id }, cancellationToken);
        if (token != null)
        {
            _context.RefreshTokens.Remove(token);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, string ipAddress, string reason, CancellationToken cancellationToken = default)
    {
        var tokens = await GetActiveTokensByUserIdAsync(userId, cancellationToken);
        foreach (var token in tokens)
        {
            token.Revoke(ipAddress, reason);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.IsExpired)
            .ToListAsync(cancellationToken);

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync(cancellationToken);
        return expiredTokens.Count;
    }

    public async Task<Dictionary<string, int>> GetTokenUsageStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var stats = new Dictionary<string, int>();
        
        var totalTokens = await _context.RefreshTokens
            .CountAsync(rt => rt.UserId == userId, cancellationToken);
        var activeTokens = await _context.RefreshTokens
            .CountAsync(rt => rt.UserId == userId && rt.IsActive, cancellationToken);
        var expiredTokens = await _context.RefreshTokens
            .CountAsync(rt => rt.UserId == userId && rt.IsExpired, cancellationToken);
        var revokedTokens = await _context.RefreshTokens
            .CountAsync(rt => rt.UserId == userId && rt.IsRevoked, cancellationToken);
        
        stats["total"] = totalTokens;
        stats["active"] = activeTokens;
        stats["expired"] = expiredTokens;
        stats["revoked"] = revokedTokens;
        
        return stats;
    }
}
