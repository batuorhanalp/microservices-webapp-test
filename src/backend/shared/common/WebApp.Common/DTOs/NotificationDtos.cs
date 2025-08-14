using System.ComponentModel.DataAnnotations;
using WebApp.Common.Entities;

namespace WebApp.Common.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? TriggerUserId { get; set; }
    public string TriggerUserName { get; set; } = string.Empty;
    public string TriggerUserDisplayName { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
}

public class CreateNotificationRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public NotificationType Type { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public Guid? EntityId { get; set; }
    
    [StringLength(50)]
    public string? EntityType { get; set; }
    
    public Guid? TriggerUserId { get; set; }
    
    [Url]
    [StringLength(500)]
    public string? ActionUrl { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}

public class UpdateNotificationStatusRequest
{
    [Required]
    public NotificationStatus Status { get; set; }
}

public class NotificationQueryRequest
{
    public NotificationType? Type { get; set; }
    public NotificationStatus? Status { get; set; }
    public bool IncludeExpired { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "desc";
}

public class NotificationStatsDto
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int ReadCount { get; set; }
    public int ArchivedCount { get; set; }
    public Dictionary<NotificationType, int> CountByType { get; set; } = new();
}

public class BulkNotificationRequest
{
    [Required]
    public List<Guid> UserIds { get; set; } = new();
    
    [Required]
    public NotificationType Type { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public Guid? EntityId { get; set; }
    
    [StringLength(50)]
    public string? EntityType { get; set; }
    
    public Guid? TriggerUserId { get; set; }
    
    [Url]
    [StringLength(500)]
    public string? ActionUrl { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}

public class MarkAllAsReadRequest
{
    public NotificationType? Type { get; set; }
    public DateTime? OlderThan { get; set; }
}
