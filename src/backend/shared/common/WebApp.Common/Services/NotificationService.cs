using Microsoft.Extensions.Logging;
using WebApp.Common.DTOs;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.Common.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NotificationDto?> GetNotificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
            return notification == null ? null : MapToDto(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification {NotificationId}", id);
            throw;
        }
    }

    public async Task<NotificationDto> CreateNotificationAsync(
        CreateNotificationRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new Notification(
                request.UserId,
                request.Type,
                request.Title,
                request.Message,
                request.EntityId,
                request.EntityType,
                request.TriggerUserId,
                request.ActionUrl,
                request.ExpiresAt);

            if (request.Metadata != null)
            {
                foreach (var metadata in request.Metadata)
                {
                    notification.AddMetadata(metadata.Key, metadata.Value);
                }
            }

            var createdNotification = await _notificationRepository.CreateAsync(notification, cancellationToken);
            
            _logger.LogInformation("Created notification {NotificationId} for user {UserId}", 
                createdNotification.Id, request.UserId);
                
            return MapToDto(createdNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationDto>> CreateBulkNotificationAsync(
        BulkNotificationRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notifications = new List<Notification>();
            
            foreach (var userId in request.UserIds)
            {
                var notification = new Notification(
                    userId,
                    request.Type,
                    request.Title,
                    request.Message,
                    request.EntityId,
                    request.EntityType,
                    request.TriggerUserId,
                    request.ActionUrl,
                    request.ExpiresAt);

                if (request.Metadata != null)
                {
                    foreach (var metadata in request.Metadata)
                    {
                        notification.AddMetadata(metadata.Key, metadata.Value);
                    }
                }
                
                notifications.Add(notification);
            }

            var createdNotifications = await _notificationRepository.CreateBulkAsync(notifications, cancellationToken);
            
            _logger.LogInformation("Created {Count} bulk notifications for {UserCount} users", 
                notifications.Count, request.UserIds.Count);
                
            return createdNotifications.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk notifications for {UserCount} users", request.UserIds.Count);
            throw;
        }
    }

    public async Task<bool> DeleteNotificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _notificationRepository.DeleteAsync(id, cancellationToken);
            
            if (result)
            {
                _logger.LogInformation("Deleted notification {NotificationId}", id);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(
        Guid userId, 
        NotificationQueryRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, request, cancellationToken);
            return notifications.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
            throw;
        }
    }

    public async Task<NotificationStatsDto> GetUserNotificationStatsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _notificationRepository.GetUserNotificationStatsAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification stats for user {UserId}", userId);
            throw;
        }
    }

    public async Task<NotificationDto?> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
            if (notification == null)
                return null;

            notification.MarkAsRead();
            var updatedNotification = await _notificationRepository.UpdateAsync(notification, cancellationToken);
            
            _logger.LogDebug("Marked notification {NotificationId} as read", id);
            
            return MapToDto(updatedNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            throw;
        }
    }

    public async Task<NotificationDto?> MarkAsUnreadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
            if (notification == null)
                return null;

            notification.MarkAsUnread();
            var updatedNotification = await _notificationRepository.UpdateAsync(notification, cancellationToken);
            
            _logger.LogDebug("Marked notification {NotificationId} as unread", id);
            
            return MapToDto(updatedNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as unread", id);
            throw;
        }
    }

    public async Task<NotificationDto?> ArchiveNotificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
            if (notification == null)
                return null;

            notification.Archive();
            var updatedNotification = await _notificationRepository.UpdateAsync(notification, cancellationToken);
            
            _logger.LogDebug("Archived notification {NotificationId}", id);
            
            return MapToDto(updatedNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving notification {NotificationId}", id);
            throw;
        }
    }

    public async Task<NotificationDto?> UnarchiveNotificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
            if (notification == null)
                return null;

            notification.Unarchive();
            var updatedNotification = await _notificationRepository.UpdateAsync(notification, cancellationToken);
            
            _logger.LogDebug("Unarchived notification {NotificationId}", id);
            
            return MapToDto(updatedNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unarchiving notification {NotificationId}", id);
            throw;
        }
    }

    public async Task<int> MarkAllAsReadAsync(
        Guid userId, 
        MarkAllAsReadRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _notificationRepository.MarkAllAsReadAsync(
                userId, 
                request.Type, 
                request.OlderThan, 
                cancellationToken);
                
            _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", count, userId);
            
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> ArchiveAllAsync(
        Guid userId, 
        NotificationType? type = null, 
        DateTime? olderThan = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _notificationRepository.MarkAllAsArchivedAsync(userId, type, olderThan, cancellationToken);
            
            _logger.LogInformation("Archived {Count} notifications for user {UserId}", count, userId);
            
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving all notifications for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId, cancellationToken);
            return notifications.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> CleanupExpiredNotificationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _notificationRepository.DeleteExpiredNotificationsAsync(cancellationToken);
            
            _logger.LogInformation("Cleaned up {Count} expired notifications", count);
            
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired notifications");
            throw;
        }
    }

    public async Task<int> CleanupOldArchivedNotificationsAsync(
        int daysOld = 90, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var olderThan = DateTime.UtcNow.AddDays(-daysOld);
            var count = await _notificationRepository.DeleteArchivedNotificationsAsync(olderThan, cancellationToken);
            
            _logger.LogInformation("Cleaned up {Count} archived notifications older than {DaysOld} days", count, daysOld);
            
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old archived notifications");
            throw;
        }
    }

    // Predefined notification creators
    public async Task<NotificationDto> CreateLikeNotificationAsync(
        Guid userId, 
        Guid postId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default)
    {
        var triggerUser = await _userRepository.GetByIdAsync(triggerUserId);
        var triggerUserName = triggerUser?.DisplayName ?? "Someone";
        
        var request = new CreateNotificationRequest
        {
            UserId = userId,
            Type = NotificationType.Like,
            Title = "New Like",
            Message = $"{triggerUserName} liked your post",
            EntityId = postId,
            EntityType = "Post",
            TriggerUserId = triggerUserId,
            ActionUrl = $"/posts/{postId}"
        };

        return await CreateNotificationAsync(request, cancellationToken);
    }

    public async Task<NotificationDto> CreateCommentNotificationAsync(
        Guid userId, 
        Guid postId, 
        Guid commentId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default)
    {
        var triggerUser = await _userRepository.GetByIdAsync(triggerUserId);
        var triggerUserName = triggerUser?.DisplayName ?? "Someone";
        
        var request = new CreateNotificationRequest
        {
            UserId = userId,
            Type = NotificationType.Comment,
            Title = "New Comment",
            Message = $"{triggerUserName} commented on your post",
            EntityId = commentId,
            EntityType = "Comment",
            TriggerUserId = triggerUserId,
            ActionUrl = $"/posts/{postId}#comment-{commentId}",
            Metadata = new Dictionary<string, object> { { "postId", postId } }
        };

        return await CreateNotificationAsync(request, cancellationToken);
    }

    public async Task<NotificationDto> CreateFollowNotificationAsync(
        Guid userId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default)
    {
        var triggerUser = await _userRepository.GetByIdAsync(triggerUserId);
        var triggerUserName = triggerUser?.DisplayName ?? "Someone";
        
        var request = new CreateNotificationRequest
        {
            UserId = userId,
            Type = NotificationType.Follow,
            Title = "New Follower",
            Message = $"{triggerUserName} started following you",
            EntityId = triggerUserId,
            EntityType = "User",
            TriggerUserId = triggerUserId,
            ActionUrl = $"/users/{triggerUserId}"
        };

        return await CreateNotificationAsync(request, cancellationToken);
    }

    public async Task<NotificationDto> CreateMentionNotificationAsync(
        Guid userId, 
        Guid postId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default)
    {
        var triggerUser = await _userRepository.GetByIdAsync(triggerUserId);
        var triggerUserName = triggerUser?.DisplayName ?? "Someone";
        
        var request = new CreateNotificationRequest
        {
            UserId = userId,
            Type = NotificationType.Mention,
            Title = "You were mentioned",
            Message = $"{triggerUserName} mentioned you in a post",
            EntityId = postId,
            EntityType = "Post",
            TriggerUserId = triggerUserId,
            ActionUrl = $"/posts/{postId}"
        };

        return await CreateNotificationAsync(request, cancellationToken);
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Status = notification.Status,
            Title = notification.Title,
            Message = notification.Message,
            EntityId = notification.EntityId,
            EntityType = notification.EntityType,
            TriggerUserId = notification.TriggerUserId,
            TriggerUserName = notification.TriggerUser?.Username ?? string.Empty,
            TriggerUserDisplayName = notification.TriggerUser?.DisplayName ?? string.Empty,
            ActionUrl = notification.ActionUrl,
            Metadata = notification.Metadata,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            ArchivedAt = notification.ArchivedAt,
            ExpiresAt = notification.ExpiresAt,
            IsExpired = notification.IsExpired()
        };
    }
}
