using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Entities;

public class User
{
    public Guid Id { get; private set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; private set; } = string.Empty;
    
    [Required]
    public string Username { get; private set; } = string.Empty;
    
    [Required]
    public string DisplayName { get; private set; } = string.Empty;
    
    public string Bio { get; private set; } = string.Empty;
    
    public string ProfileImageUrl { get; private set; } = string.Empty;
    
    public string CoverImageUrl { get; private set; } = string.Empty;
    
    public string Website { get; private set; } = string.Empty;
    
    public string Location { get; private set; } = string.Empty;
    
    public DateTime BirthDate { get; private set; }
    
    public bool IsPrivate { get; private set; }
    
    public bool IsVerified { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    // Navigation properties - Removed virtual for performance, use explicit loading when needed
    public ICollection<Post> Posts { get; private set; } = new List<Post>();
    
    public ICollection<Follow> Followers { get; private set; } = new List<Follow>();
    
    public ICollection<Follow> Following { get; private set; } = new List<Follow>();
    
    // Performance: Don't load these collections by default - use explicit queries
    // public ICollection<Like> Likes { get; private set; } = new List<Like>();
    // public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    // public ICollection<Message> SentMessages { get; private set; } = new List<Message>();
    // public ICollection<Message> ReceivedMessages { get; private set; } = new List<Message>();

    // Constructors
    private User() { } // For EF Core
    
    public User(string email, string username, string displayName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
            
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));
            
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        Username = username.ToLowerInvariant();
        DisplayName = displayName;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Domain methods
    public void UpdateProfile(string displayName, string bio, string website, string location)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            DisplayName = displayName;
            
        Bio = bio ?? string.Empty;
        Website = website ?? string.Empty;
        Location = location ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateProfileImage(string profileImageUrl)
    {
        ProfileImageUrl = profileImageUrl ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateCoverImage(string coverImageUrl)
    {
        CoverImageUrl = coverImageUrl ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetPrivacyStatus(bool isPrivate)
    {
        IsPrivate = isPrivate;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetVerificationStatus(bool isVerified)
    {
        IsVerified = isVerified;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetBirthDate(DateTime birthDate)
    {
        if (birthDate > DateTime.UtcNow.AddYears(-13))
            throw new ArgumentException("User must be at least 13 years old");
            
        BirthDate = birthDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
