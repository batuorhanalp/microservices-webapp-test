namespace WebApp.Core.Entities;

public class Share
{
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }
    
    public Guid PostId { get; private set; }
    
    public string? Comment { get; private set; } // Optional comment when sharing
    
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public virtual User User { get; private set; } = null!;
    
    public virtual Post Post { get; private set; } = null!;

    // Constructors
    private Share() { } // For EF Core
    
    public Share(Guid userId, Guid postId, string? comment = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
            
        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID is required", nameof(postId));

        Id = Guid.NewGuid();
        UserId = userId;
        PostId = postId;
        Comment = comment?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    // Domain methods
    public void UpdateComment(string? comment)
    {
        Comment = comment?.Trim();
    }
}
