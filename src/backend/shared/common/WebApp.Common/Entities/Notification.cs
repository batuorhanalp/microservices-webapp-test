using System.ComponentModel.DataAnnotations;

namespace WebApp.Common.Entities;

public enum NotificationType
{
    Like = 1,
    Comment = 2,
    Follow = 3,
    Mention = 4,
    Post = 5,
    System = 6
}

public enum NotificationStatus
{
    Unread = 1,
    Read = 2,
    Archived = 3
}

public class Notification
{
    public Guid Id { get; private set; }
    
    [Required]
    public Guid UserId { get; private set; }
    
    [Required]
    public NotificationType Type { get; private set; }
    
    [Required]
    public NotificationStatus Status { get; protected set; } = NotificationStatus.Unread;
    
    [Required]
    public string Title { get; private set; } = string.Empty;
    
    [Required]
    public string Message { get; private set; } = string.Empty;
    
    // Optional: Reference to the entity that triggered this notification
    public Guid? EntityId { get; private set; }
    
    public string EntityType { get; private set; } = string.Empty;
    
    // Optional: User who triggered this notification (e.g., who liked your post)
    public Guid? TriggerUserId { get; private set; }
    
    public string ActionUrl { get; protected set; } = string.Empty;
    
    public Dictionary<string, object> Metadata { get; private set; } = new();
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? ReadAt { get; protected set; }
    
    public DateTime? ArchivedAt { get; protected set; }
    
    public DateTime? ExpiresAt { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    
    public User? TriggerUser { get; private set; }

    // Constructors
    private Notification() { } // For EF Core
    
    public Notification(
        Guid userId, 
        NotificationType type, 
        string title, 
        string message,
        Guid? entityId = null,
        string? entityType = null,
        Guid? triggerUserId = null,
        string? actionUrl = null,
        DateTime? expiresAt = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
            
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
            
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required", nameof(message));

        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        EntityId = entityId;
        EntityType = entityType ?? string.Empty;
        TriggerUserId = triggerUserId;
        ActionUrl = actionUrl ?? string.Empty;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        Status = NotificationStatus.Unread;
    }

    // Domain methods
    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Unread)
        {
            Status = NotificationStatus.Read;
            ReadAt = DateTime.UtcNow;
        }
    }
    
    public void MarkAsUnread()
    {
        Status = NotificationStatus.Unread;
        ReadAt = null;
    }
    
    public void Archive()
    {
        Status = NotificationStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
        
        // If it wasn't read before archiving, mark it as read
        if (ReadAt == null)
        {
            ReadAt = DateTime.UtcNow;
        }
    }
    
    public void Unarchive()
    {
        Status = NotificationStatus.Read; // Unarchived notifications are marked as read
        ArchivedAt = null;
    }
    
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
    
    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
    }
    
    public T? GetMetadata<T>(string key)
    {
        if (Metadata.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }
    
    public void UpdateActionUrl(string actionUrl)
    {
        ActionUrl = actionUrl ?? string.Empty;
    }
}
