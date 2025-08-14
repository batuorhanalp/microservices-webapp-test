using WebApp.Common.DTOs;
using WebApp.Common.Entities;

namespace WebApp.Common.Interfaces;

public interface INotificationService
{
    // Basic operations
    Task<NotificationDto?> GetNotificationAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<NotificationDto> CreateNotificationAsync(
        CreateNotificationRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<NotificationDto>> CreateBulkNotificationAsync(
        BulkNotificationRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteNotificationAsync(Guid id, CancellationToken cancellationToken = default);
    
    // User notifications
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(
        Guid userId, 
        NotificationQueryRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<NotificationStatsDto> GetUserNotificationStatsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
    
    // Status management
    Task<NotificationDto?> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationDto?> MarkAsUnreadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationDto?> ArchiveNotificationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationDto?> UnarchiveNotificationAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Bulk status operations
    Task<int> MarkAllAsReadAsync(
        Guid userId, 
        MarkAllAsReadRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<int> ArchiveAllAsync(
        Guid userId, 
        NotificationType? type = null, 
        DateTime? olderThan = null, 
        CancellationToken cancellationToken = default);
    
    // Real-time operations
    Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
    
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Utility operations
    Task<int> CleanupExpiredNotificationsAsync(CancellationToken cancellationToken = default);
    
    Task<int> CleanupOldArchivedNotificationsAsync(
        int daysOld = 90, 
        CancellationToken cancellationToken = default);
    
    // Predefined notification creators
    Task<NotificationDto> CreateLikeNotificationAsync(
        Guid userId, 
        Guid postId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default);
    
    Task<NotificationDto> CreateCommentNotificationAsync(
        Guid userId, 
        Guid postId, 
        Guid commentId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default);
    
    Task<NotificationDto> CreateFollowNotificationAsync(
        Guid userId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default);
    
    Task<NotificationDto> CreateMentionNotificationAsync(
        Guid userId, 
        Guid postId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default);
}
