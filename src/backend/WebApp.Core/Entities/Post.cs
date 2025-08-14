using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Entities;

public enum PostType
{
    Text = 0,
    Image = 1,
    Video = 2,
    Audio = 3,
    Mixed = 4 // For posts with multiple media types
}

public enum PostVisibility
{
    Public = 0,
    Followers = 1,
    Private = 2
}

public class Post
{
    public Guid Id { get; private set; }
    
    public Guid AuthorId { get; private set; }
    
    public string Content { get; private set; } = string.Empty;
    
    public PostType Type { get; private set; }
    
    public PostVisibility Visibility { get; private set; }
    
    public bool IsEdited { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    // Reply functionality
    public Guid? ParentPostId { get; private set; }
    
    public Guid? RootPostId { get; private set; } // For threading
    
    // Media handling
    public virtual ICollection<MediaAttachment> MediaAttachments { get; private set; } = new List<MediaAttachment>();
    
    // Navigation properties
    public virtual User Author { get; private set; } = null!;
    
    public virtual Post? ParentPost { get; private set; }
    
    public virtual Post? RootPost { get; private set; }
    
    public virtual ICollection<Post> Replies { get; private set; } = new List<Post>();
    
    public virtual ICollection<Like> Likes { get; private set; } = new List<Like>();
    
    public virtual ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    
    public virtual ICollection<Share> Shares { get; private set; } = new List<Share>();

    // Constructors
    private Post() { } // For EF Core
    
    public Post(Guid authorId, string content, PostType type = PostType.Text, PostVisibility visibility = PostVisibility.Public)
    {
        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID is required", nameof(authorId));
            
        if (string.IsNullOrWhiteSpace(content) && type == PostType.Text)
            throw new ArgumentException("Content is required for text posts", nameof(content));

        Id = Guid.NewGuid();
        AuthorId = authorId;
        Content = content ?? string.Empty;
        Type = type;
        Visibility = visibility;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Reply constructor
    public Post(Guid authorId, string content, Guid parentPostId, Guid? rootPostId = null, PostVisibility visibility = PostVisibility.Public)
        : this(authorId, content, PostType.Text, visibility)
    {
        ParentPostId = parentPostId;
        RootPostId = rootPostId ?? parentPostId; // If no root specified, parent becomes root
    }

    // Domain methods
    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content) && Type == PostType.Text)
            throw new ArgumentException("Content cannot be empty for text posts", nameof(content));

        Content = content ?? string.Empty;
        IsEdited = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetVisibility(PostVisibility visibility)
    {
        Visibility = visibility;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddMediaAttachment(string url, string fileName, string contentType, long fileSize)
    {
        var attachment = new MediaAttachment(Id, url, fileName, contentType, fileSize);
        MediaAttachments.Add(attachment);
        
        // Update post type based on media
        UpdatePostType();
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool CanBeViewedBy(User viewer, User author)
    {
        if (Visibility == PostVisibility.Public)
            return true;
            
        if (viewer.Id == author.Id)
            return true;
            
        if (Visibility == PostVisibility.Private)
            return false;
            
        // Check if viewer follows the author
        return author.Followers.Any(f => f.FollowerId == viewer.Id && f.IsAccepted);
    }
    
    public bool IsReply => ParentPostId.HasValue;
    
    public int GetLikeCount() => Likes.Count;
    
    public int GetCommentCount() => Comments.Count;
    
    public int GetShareCount() => Shares.Count;
    
    public int GetReplyCount() => Replies.Count;

    private void UpdatePostType()
    {
        if (!MediaAttachments.Any())
        {
            Type = PostType.Text;
            return;
        }

        var mediaTypes = MediaAttachments.Select(m => GetMediaTypeFromContentType(m.ContentType)).Distinct().ToList();
        
        if (mediaTypes.Count > 1)
        {
            Type = PostType.Mixed;
        }
        else
        {
            Type = mediaTypes.First();
        }
    }
    
    private PostType GetMediaTypeFromContentType(string contentType)
    {
        if (contentType.StartsWith("image/"))
            return PostType.Image;
        if (contentType.StartsWith("video/"))
            return PostType.Video;
        if (contentType.StartsWith("audio/"))
            return PostType.Audio;
            
        return PostType.Text;
    }
}
