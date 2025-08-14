using Microsoft.AspNetCore.Mvc;
using WebApp.Common.DTOs;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get a notification by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationDto>> GetNotification(Guid id)
    {
        try
        {
            var notification = await _notificationService.GetNotificationAsync(id);
            return notification == null ? NotFound() : Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification {NotificationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new notification
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = await _notificationService.CreateNotificationAsync(request);
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create bulk notifications
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> CreateBulkNotifications([FromBody] BulkNotificationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notifications = await _notificationService.CreateBulkNotificationAsync(request);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk notifications");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        try
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user notifications with filtering and pagination
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserNotifications(
        Guid userId, 
        [FromQuery] NotificationQueryRequest request)
    {
        try
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, request);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user notification statistics
    /// </summary>
    [HttpGet("users/{userId}/stats")]
    public async Task<ActionResult<NotificationStatsDto>> GetUserNotificationStats(Guid userId)
    {
        try
        {
            var stats = await _notificationService.GetUserNotificationStatsAsync(userId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification stats for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get unread notifications for a user
    /// </summary>
    [HttpGet("users/{userId}/unread")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUnreadNotifications(Guid userId)
    {
        try
        {
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get unread count for a user
    /// </summary>
    [HttpGet("users/{userId}/unread/count")]
    public async Task<ActionResult<int>> GetUnreadCount(Guid userId)
    {
        try
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPatch("{id}/read")]
    public async Task<ActionResult<NotificationDto>> MarkAsRead(Guid id)
    {
        try
        {
            var notification = await _notificationService.MarkAsReadAsync(id);
            return notification == null ? NotFound() : Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark notification as unread
    /// </summary>
    [HttpPatch("{id}/unread")]
    public async Task<ActionResult<NotificationDto>> MarkAsUnread(Guid id)
    {
        try
        {
            var notification = await _notificationService.MarkAsUnreadAsync(id);
            return notification == null ? NotFound() : Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as unread", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Archive notification
    /// </summary>
    [HttpPatch("{id}/archive")]
    public async Task<ActionResult<NotificationDto>> ArchiveNotification(Guid id)
    {
        try
        {
            var notification = await _notificationService.ArchiveNotificationAsync(id);
            return notification == null ? NotFound() : Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving notification {NotificationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Unarchive notification
    /// </summary>
    [HttpPatch("{id}/unarchive")]
    public async Task<ActionResult<NotificationDto>> UnarchiveNotification(Guid id)
    {
        try
        {
            var notification = await _notificationService.UnarchiveNotificationAsync(id);
            return notification == null ? NotFound() : Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unarchiving notification {NotificationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark all notifications as read for a user
    /// </summary>
    [HttpPatch("users/{userId}/read-all")]
    public async Task<ActionResult<int>> MarkAllAsRead(Guid userId, [FromBody] MarkAllAsReadRequest request)
    {
        try
        {
            var count = await _notificationService.MarkAllAsReadAsync(userId, request);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Archive all notifications for a user
    /// </summary>
    [HttpPatch("users/{userId}/archive-all")]
    public async Task<ActionResult<int>> ArchiveAll(Guid userId, [FromQuery] NotificationType? type = null, [FromQuery] DateTime? olderThan = null)
    {
        try
        {
            var count = await _notificationService.ArchiveAllAsync(userId, type, olderThan);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving all notifications for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a like notification
    /// </summary>
    [HttpPost("like")]
    public async Task<ActionResult<NotificationDto>> CreateLikeNotification([FromBody] CreateLikeNotificationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = await _notificationService.CreateLikeNotificationAsync(
                request.UserId, 
                request.PostId, 
                request.TriggerUserId);
                
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating like notification");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a comment notification
    /// </summary>
    [HttpPost("comment")]
    public async Task<ActionResult<NotificationDto>> CreateCommentNotification([FromBody] CreateCommentNotificationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = await _notificationService.CreateCommentNotificationAsync(
                request.UserId, 
                request.PostId, 
                request.CommentId, 
                request.TriggerUserId);
                
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment notification");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a follow notification
    /// </summary>
    [HttpPost("follow")]
    public async Task<ActionResult<NotificationDto>> CreateFollowNotification([FromBody] CreateFollowNotificationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = await _notificationService.CreateFollowNotificationAsync(
                request.UserId, 
                request.TriggerUserId);
                
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating follow notification");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a mention notification
    /// </summary>
    [HttpPost("mention")]
    public async Task<ActionResult<NotificationDto>> CreateMentionNotification([FromBody] CreateMentionNotificationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = await _notificationService.CreateMentionNotificationAsync(
                request.UserId, 
                request.PostId, 
                request.TriggerUserId);
                
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating mention notification");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Admin endpoint to cleanup expired notifications
    /// </summary>
    [HttpDelete("expired")]
    public async Task<ActionResult<int>> CleanupExpiredNotifications()
    {
        try
        {
            var count = await _notificationService.CleanupExpiredNotificationsAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired notifications");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Admin endpoint to cleanup old archived notifications
    /// </summary>
    [HttpDelete("archived")]
    public async Task<ActionResult<int>> CleanupOldArchivedNotifications([FromQuery] int daysOld = 90)
    {
        try
        {
            var count = await _notificationService.CleanupOldArchivedNotificationsAsync(daysOld);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old archived notifications");
            return StatusCode(500, "Internal server error");
        }
    }
}

// DTOs for predefined notification requests
public class CreateLikeNotificationRequest
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid TriggerUserId { get; set; }
}

public class CreateCommentNotificationRequest
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid CommentId { get; set; }
    public Guid TriggerUserId { get; set; }
}

public class CreateFollowNotificationRequest
{
    public Guid UserId { get; set; }
    public Guid TriggerUserId { get; set; }
}

public class CreateMentionNotificationRequest
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid TriggerUserId { get; set; }
}
