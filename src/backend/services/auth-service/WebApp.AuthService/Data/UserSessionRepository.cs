using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.AuthService.Data;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly AuthDbContext _context;

    public UserSessionRepository(AuthDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(us => us.SessionId == sessionId, cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .Where(us => us.UserId == userId && us.IsValidSession)
            .OrderByDescending(us => us.LastActivityAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSession> CreateAsync(UserSession userSession, CancellationToken cancellationToken = default)
    {
        _context.UserSessions.Add(userSession);
        await _context.SaveChangesAsync(cancellationToken);
        return userSession;
    }

    public async Task<UserSession> UpdateAsync(UserSession userSession, CancellationToken cancellationToken = default)
    {
        _context.UserSessions.Update(userSession);
        await _context.SaveChangesAsync(cancellationToken);
        return userSession;
    }

    public async Task DeactivateSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await GetBySessionIdAsync(sessionId, cancellationToken);
        if (session != null)
        {
            session.Terminate();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeactivateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await GetActiveSessionsByUserIdAsync(userId, cancellationToken);
        foreach (var session in sessions)
        {
            session.Terminate();
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        var expiredSessions = await _context.UserSessions
            .Where(us => us.IsExpired)
            .ToListAsync(cancellationToken);

        _context.UserSessions.RemoveRange(expiredSessions);
        await _context.SaveChangesAsync(cancellationToken);
        return expiredSessions.Count;
    }

    public async Task UpdateActivityAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await GetBySessionIdAsync(sessionId, cancellationToken);
        if (session?.IsValidSession == true)
        {
            session.UpdateActivity();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
