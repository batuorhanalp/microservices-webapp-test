using WebApp.Common.DTOs;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.NotificationService.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IPostRepository postRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _postRepository = postRepository;
        _logger = logger;
    }

    public async Task<NotificationDto?> GetNotificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        return notification != null ? MapToDto(notification) : null;
    }

    public async Task<NotificationDto> CreateNotificationAsync(
        CreateNotificationRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(request.UserId));
        }

        // Validate trigger user if provided
        if (request.TriggerUserId.HasValue)
        {
            var triggerUser = await _userRepository.GetByIdAsync(request.TriggerUserId.Value);
            if (triggerUser == null)
            {
                throw new ArgumentException("Trigger user not found", nameof(request.TriggerUserId));
            }
        }

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

        // Add metadata if provided
        if (request.Metadata != null)
        {
            foreach (var kvp in request.Metadata)
            {
                notification.AddMetadata(kvp.Key, kvp.Value);
            }
        }

        var createdNotification = await _notificationRepository.CreateAsync(notification, cancellationToken);

        _logger.LogInformation("Created notification {NotificationId} for user {UserId}", createdNotification.Id, createdNotification.UserId);

        return MapToDto(createdNotification);
    }

    public async Task<IEnumerable<NotificationDto>> CreateBulkNotificationAsync(
        BulkNotificationRequest request, 
        CancellationToken cancellationToken = default)
    {
        var notifications = new List<Notification>();

        foreach (var userId in request.UserIds)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Skipping notification for non-existent user {UserId}", userId);
                continue;
            }

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

            // Add metadata if provided
            if (request.Metadata != null)
            {
                foreach (var kvp in request.Metadata)
                {
                    notification.AddMetadata(kvp.Key, kvp.Value);
                }
            }

            notifications.Add(notification);
        }

        if (notifications.Any())
        {
            var createdNotifications = await _notificationRepository.CreateBulkAsync(notifications, cancellationToken);
            _logger.LogInformation("Created {Count} bulk notifications", notifications.Count);
            return createdNotifications.Select(MapToDto);
        }

        return Enumerable.Empty<NotificationDto>();
    }

    public async Task<bool> DeleteNotificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _notificationRepository.DeleteAsync(id, cancellationToken);
        
        if (result)
        {
            _logger.LogInformation("Deleted notification {NotificationId}", id);
        }
        
        return result;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(
        Guid userId, 
        NotificationQueryRequest request, 
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, request, cancellationToken);
        return notifications.Select(MapToDto);
    }

    public async Task<NotificationStatsDto> GetUserNotificationStatsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetUserNotificationStatsAsync(userId, cancellationToken);
    }

    public async Task<NotificationDto?> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        if (notification == null)
        {
            return null;
        }

        notification.MarkAsRead();
        var updatedNotification = await _notificationRepository.UpdateAsync(notification, cancellationToken);

        _logger.LogInformation("Marked notification {NotificationId} as read", id);

        return MapToDto(updatedNotification);
    }

    public async Task<NotificationDto?> MarkAsUnreadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        if (notification == null)
        {
            return null;
        }

        notification.MarkAsUnread();
        var updatedNotification = await _notificationRepository.UpdateAsync(notification, cancellationToken);

        _logger.LogInformation("Marked notification {NotificationId} as unread", id);

        return MapToDto(updatedNotification);
    }

    public async Task<NotificationDto?> ArchiveNotificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        if (notification == null)
        {
            return null;
        }

        notification.Archive();
        var updatedNotification = await _notificationRepository.UpdateAsync(notification, cancellationToken);

        _logger.LogInformation("Archived notification {NotificationId}", id);

        return MapToDto(updatedNotification);
    }

    public async Task<NotificationDto?> UnarchiveNotificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        if (notification == null)
        {
            return null;
        }

        notification.Unarchive();
        var updatedNotification = await _notificationRepository.UpdateAsync(notification, cancellationToken);

        _logger.LogInformation("Unarchived notification {NotificationId}", id);

        return MapToDto(updatedNotification);
    }

    public async Task<int> MarkAllAsReadAsync(
        Guid userId, 
        MarkAllAsReadRequest request, 
        CancellationToken cancellationToken = default)
    {
        var count = await _notificationRepository.MarkAllAsReadAsync(userId, request.Type, request.OlderThan, cancellationToken);
        _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", count, userId);
        return count;
    }

    public async Task<int> ArchiveAllAsync(
        Guid userId, 
        NotificationType? type = null, 
        DateTime? olderThan = null, 
        CancellationToken cancellationToken = default)
    {
        var count = await _notificationRepository.MarkAllAsArchivedAsync(userId, type, olderThan, cancellationToken);
        _logger.LogInformation("Archived {Count} notifications for user {UserId}", count, userId);
        return count;
    }

    public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId, cancellationToken);
        return notifications.Select(MapToDto);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task<int> CleanupExpiredNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var count = await _notificationRepository.DeleteExpiredNotificationsAsync(cancellationToken);
        _logger.LogInformation("Cleaned up {Count} expired notifications", count);
        return count;
    }

    public async Task<int> CleanupOldArchivedNotificationsAsync(
        int daysOld = 90, 
        CancellationToken cancellationToken = default)
    {
        var olderThan = DateTime.UtcNow.AddDays(-daysOld);
        var count = await _notificationRepository.DeleteArchivedNotificationsAsync(olderThan, cancellationToken);
        _logger.LogInformation("Cleaned up {Count} old archived notifications", count);
        return count;
    }

    public async Task<NotificationDto> CreateLikeNotificationAsync(
        Guid userId, 
        Guid postId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default)
    {
        // Validate users and post exist
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(userId));
        }

        var triggerUser = await _userRepository.GetByIdAsync(triggerUserId);
        if (triggerUser == null)
        {
            throw new ArgumentException("Trigger user not found", nameof(triggerUserId));
        }

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ArgumentException("Post not found", nameof(postId));
        }

        // Don't notify users of their own actions
        if (userId == triggerUserId)
        {
            throw new InvalidOperationException("Cannot create notification for user's own action");
        }

        var notification = new Notification(
            userId,
            NotificationType.Like,
            "New Like",
            $"{triggerUser.DisplayName} liked your post",
            postId,
            "Post",
            triggerUserId,
            $"/posts/{postId}");

        // Add metadata
        notification.AddMetadata("postId", postId);
        notification.AddMetadata("triggerUserId", triggerUserId);
        notification.AddMetadata("triggerUserDisplayName", triggerUser.DisplayName);
        notification.AddMetadata("triggerUserUsername", triggerUser.Username);

        var createdNotification = await _notificationRepository.CreateAsync(notification, cancellationToken);

        _logger.LogInformation("Created like notification {NotificationId} for user {UserId} from {TriggerUserId}", 
            createdNotification.Id, userId, triggerUserId);

        return MapToDto(createdNotification);
    }

    public async Task<NotificationDto> CreateCommentNotificationAsync(
        Guid userId, 
        Guid postId, 
        Guid commentId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default)
    {
        // Validate users and post exist
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(userId));
        }

        var triggerUser = await _userRepository.GetByIdAsync(triggerUserId);
        if (triggerUser == null)
        {
            throw new ArgumentException("Trigger user not found", nameof(triggerUserId));
        }

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ArgumentException("Post not found", nameof(postId));
        }

        // Don't notify users of their own actions
        if (userId == triggerUserId)
        {
            throw new InvalidOperationException("Cannot create notification for user's own action");
        }

        var notification = new Notification(
            userId,
            NotificationType.Comment,
            "New Comment",
            $"{triggerUser.DisplayName} commented on your post",
            postId,
            "Post",
            triggerUserId,
            $"/posts/{postId}#comment-{commentId}");

        // Add metadata
        notification.AddMetadata("postId", postId);
        notification.AddMetadata("commentId", commentId);
        notification.AddMetadata("triggerUserId", triggerUserId);
        notification.AddMetadata("triggerUserDisplayName", triggerUser.DisplayName);
        notification.AddMetadata("triggerUserUsername", triggerUser.Username);

        var createdNotification = await _notificationRepository.CreateAsync(notification, cancellationToken);

        _logger.LogInformation("Created comment notification {NotificationId} for user {UserId} from {TriggerUserId}", 
            createdNotification.Id, userId, triggerUserId);

        return MapToDto(createdNotification);
    }

    public async Task<NotificationDto> CreateFollowNotificationAsync(
        Guid userId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default)
    {
        // Validate users exist
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(userId));
        }

        var triggerUser = await _userRepository.GetByIdAsync(triggerUserId);
        if (triggerUser == null)
        {
            throw new ArgumentException("Trigger user not found", nameof(triggerUserId));
        }

        // Don't notify users of their own actions
        if (userId == triggerUserId)
        {
            throw new InvalidOperationException("Cannot create notification for user's own action");
        }

        var notification = new Notification(
            userId,
            NotificationType.Follow,
            "New Follower",
            $"{triggerUser.DisplayName} started following you",
            triggerUserId,
            "User",
            triggerUserId,
            $"/users/{triggerUser.Username}");

        // Add metadata
        notification.AddMetadata("triggerUserId", triggerUserId);
        notification.AddMetadata("triggerUserDisplayName", triggerUser.DisplayName);
        notification.AddMetadata("triggerUserUsername", triggerUser.Username);

        var createdNotification = await _notificationRepository.CreateAsync(notification, cancellationToken);

        _logger.LogInformation("Created follow notification {NotificationId} for user {UserId} from {TriggerUserId}", 
            createdNotification.Id, userId, triggerUserId);

        return MapToDto(createdNotification);
    }

    public async Task<NotificationDto> CreateMentionNotificationAsync(
        Guid userId, 
        Guid postId, 
        Guid triggerUserId, 
        CancellationToken cancellationToken = default)
    {
        // Validate users and post exist
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(userId));
        }

        var triggerUser = await _userRepository.GetByIdAsync(triggerUserId);
        if (triggerUser == null)
        {
            throw new ArgumentException("Trigger user not found", nameof(triggerUserId));
        }

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ArgumentException("Post not found", nameof(postId));
        }

        // Don't notify users of their own actions
        if (userId == triggerUserId)
        {
            throw new InvalidOperationException("Cannot create notification for user's own action");
        }

        var notification = new Notification(
            userId,
            NotificationType.Mention,
            "Mention",
            $"{triggerUser.DisplayName} mentioned you in a post",
            postId,
            "Post",
            triggerUserId,
            $"/posts/{postId}");

        // Add metadata
        notification.AddMetadata("postId", postId);
        notification.AddMetadata("triggerUserId", triggerUserId);
        notification.AddMetadata("triggerUserDisplayName", triggerUser.DisplayName);
        notification.AddMetadata("triggerUserUsername", triggerUser.Username);

        var createdNotification = await _notificationRepository.CreateAsync(notification, cancellationToken);

        _logger.LogInformation("Created mention notification {NotificationId} for user {UserId} from {TriggerUserId}", 
            createdNotification.Id, userId, triggerUserId);

        return MapToDto(createdNotification);
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            TriggerUserId = notification.TriggerUserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            Status = notification.Status,
            EntityType = notification.EntityType,
            EntityId = notification.EntityId,
            ActionUrl = notification.ActionUrl,
            ExpiresAt = notification.ExpiresAt,
            Metadata = notification.Metadata,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            ArchivedAt = notification.ArchivedAt,
            IsExpired = notification.IsExpired()
        };
    }
}
