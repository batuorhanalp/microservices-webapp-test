namespace WebApp.Core.Entities;

public class Like
{
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }
    
    public Guid PostId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public virtual User User { get; private set; } = null!;
    
    public virtual Post Post { get; private set; } = null!;

    // Constructors
    private Like() { } // For EF Core
    
    public Like(Guid userId, Guid postId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
            
        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID is required", nameof(postId));

        Id = Guid.NewGuid();
        UserId = userId;
        PostId = postId;
        CreatedAt = DateTime.UtcNow;
    }
}
