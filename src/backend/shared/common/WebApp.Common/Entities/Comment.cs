using System.ComponentModel.DataAnnotations;

namespace WebApp.Common.Entities;

public class Comment
{
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }
    
    public Guid PostId { get; private set; }
    
    [Required]
    public string Content { get; private set; } = string.Empty;
    
    public bool IsEdited { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    
    public Post Post { get; private set; } = null!;

    // Constructors
    private Comment() { } // For EF Core
    
    public Comment(Guid userId, Guid postId, string content)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));
            
        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID is required", nameof(postId));
            
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content is required", nameof(content));

        Id = Guid.NewGuid();
        UserId = userId;
        PostId = postId;
        Content = content.Trim();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Domain methods
    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content cannot be empty", nameof(content));

        Content = content.Trim();
        IsEdited = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
