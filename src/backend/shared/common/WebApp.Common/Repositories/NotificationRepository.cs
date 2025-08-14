using Microsoft.EntityFrameworkCore;
using WebApp.Common.Data;
using WebApp.Common.DTOs;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.Common.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Include(n => n.User)
            .Include(n => n.TriggerUser)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FindAsync(new object[] { id }, cancellationToken);
        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
        Guid userId, 
        NotificationQueryRequest request, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Include(n => n.TriggerUser)
            .Where(n => n.UserId == userId);

        // Apply filters
        if (request.Type.HasValue)
        {
            query = query.Where(n => n.Type == request.Type.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(n => n.Status == request.Status.Value);
        }

        if (!request.IncludeExpired)
        {
            query = query.Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);
        }

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "createdat" => request.SortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(n => n.CreatedAt)
                : query.OrderBy(n => n.CreatedAt),
            "readat" => request.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(n => n.ReadAt)
                : query.OrderBy(n => n.ReadAt),
            "type" => request.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(n => n.Type)
                : query.OrderBy(n => n.Type),
            _ => query.OrderByDescending(n => n.CreatedAt)
        };

        // Apply pagination
        return await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUserNotificationsCountAsync(
        Guid userId, 
        NotificationQueryRequest request, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications.Where(n => n.UserId == userId);

        if (request.Type.HasValue)
        {
            query = query.Where(n => n.Type == request.Type.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(n => n.Status == request.Status.Value);
        }

        if (!request.IncludeExpired)
        {
            query = query.Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<NotificationStatsDto> GetUserNotificationStatsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .Select(n => new { n.Status, n.Type })
            .ToListAsync(cancellationToken);

        var stats = new NotificationStatsDto
        {
            TotalCount = notifications.Count,
            UnreadCount = notifications.Count(n => n.Status == NotificationStatus.Unread),
            ReadCount = notifications.Count(n => n.Status == NotificationStatus.Read),
            ArchivedCount = notifications.Count(n => n.Status == NotificationStatus.Archived)
        };

        // Count by type
        foreach (var group in notifications.GroupBy(n => n.Type))
        {
            stats.CountByType[group.Key] = group.Count();
        }

        return stats;
    }

    public async Task<IEnumerable<Notification>> CreateBulkAsync(
        IEnumerable<Notification> notifications, 
        CancellationToken cancellationToken = default)
    {
        var notificationList = notifications.ToList();
        _context.Notifications.AddRange(notificationList);
        await _context.SaveChangesAsync(cancellationToken);
        return notificationList;
    }

    public async Task<int> MarkAllAsReadAsync(
        Guid userId, 
        NotificationType? type = null, 
        DateTime? olderThan = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsTracking()
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread);

        if (type.HasValue)
        {
            query = query.Where(n => n.Type == type.Value);
        }

        if (olderThan.HasValue)
        {
            query = query.Where(n => n.CreatedAt <= olderThan.Value);
        }

        var notifications = await query.ToListAsync(cancellationToken);
        
        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }

    public async Task<int> MarkAllAsArchivedAsync(
        Guid userId, 
        NotificationType? type = null, 
        DateTime? olderThan = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsTracking()
            .Where(n => n.UserId == userId && n.Status != NotificationStatus.Archived);

        if (type.HasValue)
        {
            query = query.Where(n => n.Type == type.Value);
        }

        if (olderThan.HasValue)
        {
            query = query.Where(n => n.CreatedAt <= olderThan.Value);
        }

        var notifications = await query.ToListAsync(cancellationToken);
        
        foreach (var notification in notifications)
        {
            notification.Archive();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }

    public async Task<int> DeleteExpiredNotificationsAsync(CancellationToken cancellationToken = default)
    {
        // Get expired notification IDs to avoid tracking conflicts
        var expiredIds = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.ExpiresAt != null && n.ExpiresAt <= DateTime.UtcNow)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        if (expiredIds.Any())
        {
            // Clear change tracker to avoid conflicts
            _context.ChangeTracker.Clear();
            
            // Process deletions in batches to avoid memory issues
            const int batchSize = 100;
            for (int i = 0; i < expiredIds.Count; i += batchSize)
            {
                var batch = expiredIds.Skip(i).Take(batchSize).ToList();
                var notificationsToDelete = await _context.Notifications
                    .Where(n => batch.Contains(n.Id))
                    .ToListAsync(cancellationToken);
                
                _context.Notifications.RemoveRange(notificationsToDelete);
                await _context.SaveChangesAsync(cancellationToken);
                
                // Clear change tracker after each batch
                _context.ChangeTracker.Clear();
            }
        }
        
        return expiredIds.Count;
    }

    public async Task<int> DeleteArchivedNotificationsAsync(
        DateTime olderThan, 
        CancellationToken cancellationToken = default)
    {
        // Get archived notification IDs to avoid tracking conflicts  
        var archivedIds = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Status == NotificationStatus.Archived && n.ArchivedAt <= olderThan)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        if (archivedIds.Any())
        {
            // Clear change tracker to avoid conflicts
            _context.ChangeTracker.Clear();
            
            // Process deletions in batches to avoid memory issues
            const int batchSize = 100;
            for (int i = 0; i < archivedIds.Count; i += batchSize)
            {
                var batch = archivedIds.Skip(i).Take(batchSize).ToList();
                var notificationsToDelete = await _context.Notifications
                    .Where(n => batch.Contains(n.Id))
                    .ToListAsync(cancellationToken);
                
                _context.Notifications.RemoveRange(notificationsToDelete);
                await _context.SaveChangesAsync(cancellationToken);
                
                // Clear change tracker after each batch
                _context.ChangeTracker.Clear();
            }
        }
        
        return archivedIds.Count;
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Include(n => n.TriggerUser)
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .CountAsync(cancellationToken);
    }
}
