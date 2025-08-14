using System.ComponentModel.DataAnnotations;

namespace WebApp.Common.Entities;

public enum MessageType
{
    Text = 0,
    Image = 1,
    Video = 2,
    Audio = 3,
    File = 4
}

public class Message
{
    public Guid Id { get; private set; }
    
    public Guid SenderId { get; private set; }
    
    public Guid RecipientId { get; private set; }
    
    public string Content { get; private set; } = string.Empty;
    
    public MessageType Type { get; private set; }
    
    public string? AttachmentUrl { get; private set; }
    
    public string? AttachmentFileName { get; private set; }
    
    public long? AttachmentFileSize { get; private set; }
    
    public bool IsRead { get; private set; }
    
    public bool IsEdited { get; private set; }
    
    public bool IsDeleted { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    public DateTime? ReadAt { get; private set; }

    // Navigation properties
    public User Sender { get; private set; } = null!;
    
    public User Recipient { get; private set; } = null!;

    // Constructors
    private Message() { } // For EF Core
    
    public Message(Guid senderId, Guid recipientId, string content, MessageType type = MessageType.Text)
    {
        if (senderId == Guid.Empty)
            throw new ArgumentException("Sender ID is required", nameof(senderId));
            
        if (recipientId == Guid.Empty)
            throw new ArgumentException("Recipient ID is required", nameof(recipientId));
            
        if (senderId == recipientId)
            throw new ArgumentException("Sender and recipient cannot be the same");
            
        if (string.IsNullOrWhiteSpace(content) && type == MessageType.Text)
            throw new ArgumentException("Content is required for text messages", nameof(content));

        Id = Guid.NewGuid();
        SenderId = senderId;
        RecipientId = recipientId;
        Content = content ?? string.Empty;
        Type = type;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Domain methods
    public void UpdateContent(string content)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot edit deleted message");
            
        if (string.IsNullOrWhiteSpace(content) && Type == MessageType.Text)
            throw new ArgumentException("Content cannot be empty for text messages", nameof(content));

        Content = content ?? string.Empty;
        IsEdited = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
    
    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetAttachment(string url, string fileName, long fileSize)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Attachment URL is required", nameof(url));
            
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Attachment file name is required", nameof(fileName));
            
        if (fileSize <= 0)
            throw new ArgumentException("File size must be positive", nameof(fileSize));

        AttachmentUrl = url;
        AttachmentFileName = fileName;
        AttachmentFileSize = fileSize;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool HasAttachment => !string.IsNullOrWhiteSpace(AttachmentUrl);
}
