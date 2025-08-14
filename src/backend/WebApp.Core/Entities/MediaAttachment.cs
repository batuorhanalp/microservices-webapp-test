using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Entities;

public class MediaAttachment
{
    public Guid Id { get; private set; }
    
    public Guid PostId { get; private set; }
    
    [Required]
    public string Url { get; private set; } = string.Empty;
    
    [Required]
    public string FileName { get; private set; } = string.Empty;
    
    [Required]
    public string ContentType { get; private set; } = string.Empty;
    
    public long FileSize { get; private set; }
    
    public string? AltText { get; private set; }
    
    public int? Width { get; private set; }
    
    public int? Height { get; private set; }
    
    public int? Duration { get; private set; } // For video/audio in seconds
    
    public string? ThumbnailUrl { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Post Post { get; private set; } = null!;

    // Constructors
    private MediaAttachment() { } // For EF Core
    
    public MediaAttachment(Guid postId, string url, string fileName, string contentType, long fileSize)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID is required", nameof(postId));
            
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required", nameof(url));
            
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));
            
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required", nameof(contentType));
            
        if (fileSize <= 0)
            throw new ArgumentException("File size must be positive", nameof(fileSize));

        Id = Guid.NewGuid();
        PostId = postId;
        Url = url;
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        CreatedAt = DateTime.UtcNow;
    }

    // Domain methods
    public void SetAltText(string altText)
    {
        AltText = altText;
    }
    
    public void SetDimensions(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width and height must be positive values");
            
        Width = width;
        Height = height;
    }
    
    public void SetDuration(int duration)
    {
        if (duration <= 0)
            throw new ArgumentException("Duration must be positive", nameof(duration));
            
        Duration = duration;
    }
    
    public void SetThumbnailUrl(string thumbnailUrl)
    {
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("Thumbnail URL cannot be empty", nameof(thumbnailUrl));
            
        ThumbnailUrl = thumbnailUrl;
    }
    
    public bool IsImage => ContentType.StartsWith("image/");
    
    public bool IsVideo => ContentType.StartsWith("video/");
    
    public bool IsAudio => ContentType.StartsWith("audio/");
    
    public string GetFileSizeFormatted()
    {
        const int unit = 1024;
        if (FileSize < unit) return $"{FileSize} B";
        
        int exp = (int)(Math.Log(FileSize) / Math.Log(unit));
        string pre = "KMGTPE"[exp - 1].ToString();
        return $"{FileSize / Math.Pow(unit, exp):F1} {pre}B";
    }
}
