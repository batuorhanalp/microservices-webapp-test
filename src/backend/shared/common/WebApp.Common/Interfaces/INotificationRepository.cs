using WebApp.Common.DTOs;
using WebApp.Common.Entities;

namespace WebApp.Common.Interfaces;

public interface INotificationRepository
{
    // Basic CRUD operations
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Query operations
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(
        Guid userId, 
        NotificationQueryRequest request, 
        CancellationToken cancellationToken = default);
        
    Task<int> GetUserNotificationsCountAsync(
        Guid userId, 
        NotificationQueryRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<NotificationStatsDto> GetUserNotificationStatsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
    
    // Bulk operations
    Task<IEnumerable<Notification>> CreateBulkAsync(
        IEnumerable<Notification> notifications, 
        CancellationToken cancellationToken = default);
        
    Task<int> MarkAllAsReadAsync(
        Guid userId, 
        NotificationType? type = null, 
        DateTime? olderThan = null, 
        CancellationToken cancellationToken = default);
        
    Task<int> MarkAllAsArchivedAsync(
        Guid userId, 
        NotificationType? type = null, 
        DateTime? olderThan = null, 
        CancellationToken cancellationToken = default);
    
    // Cleanup operations
    Task<int> DeleteExpiredNotificationsAsync(CancellationToken cancellationToken = default);
    
    Task<int> DeleteArchivedNotificationsAsync(
        DateTime olderThan, 
        CancellationToken cancellationToken = default);
    
    // Real-time operations
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
        
    Task<int> GetUnreadCountAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
}
