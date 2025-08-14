using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.AuthService.Data;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AuthDbContext _context;

    public PasswordResetTokenRepository(AuthDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .Include(prt => prt.User)
            .FirstOrDefaultAsync(prt => prt.Token == token, cancellationToken);
    }

    public async Task<PasswordResetToken?> GetActiveTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(prt => prt.UserId == userId && prt.IsValid, cancellationToken);
    }

    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken passwordResetToken, CancellationToken cancellationToken = default)
    {
        _context.PasswordResetTokens.Add(passwordResetToken);
        await _context.SaveChangesAsync(cancellationToken);
        return passwordResetToken;
    }

    public async Task<PasswordResetToken> UpdateAsync(PasswordResetToken passwordResetToken, CancellationToken cancellationToken = default)
    {
        _context.PasswordResetTokens.Update(passwordResetToken);
        await _context.SaveChangesAsync(cancellationToken);
        return passwordResetToken;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var token = await _context.PasswordResetTokens.FindAsync(new object[] { id }, cancellationToken);
        if (token != null)
        {
            _context.PasswordResetTokens.Remove(token);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }

    public async Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.PasswordResetTokens
            .Where(prt => prt.UserId == userId && prt.IsValid)
            .ToListAsync(cancellationToken);
            
        foreach (var token in tokens)
        {
            token.MarkAsUsed();
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.PasswordResetTokens
            .Where(prt => prt.IsExpired)
            .ToListAsync(cancellationToken);

        _context.PasswordResetTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync(cancellationToken);
        return expiredTokens.Count;
    }
}
