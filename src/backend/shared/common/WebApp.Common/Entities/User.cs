using System.ComponentModel.DataAnnotations;

namespace WebApp.Common.Entities;

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
    
    // Authentication fields
    [Required]
    public string PasswordHash { get; private set; } = string.Empty;
    
    public string? PasswordSalt { get; private set; }
    
    public DateTime? LastLoginAt { get; private set; }
    
    public DateTime? PasswordChangedAt { get; private set; }
    
    public int FailedLoginAttempts { get; private set; }
    
    public DateTime? LockoutEndAt { get; private set; }
    
    public bool IsEmailConfirmed { get; private set; }
    
    public string? EmailConfirmationToken { get; private set; }
    
    public bool IsTwoFactorEnabled { get; private set; }
    
    public string? TwoFactorSecret { get; private set; }
    
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
    
    public User(string email, string username, string displayName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
            
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));
            
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));
            
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        Username = username.ToLowerInvariant();
        DisplayName = displayName;
        PasswordHash = passwordHash;
        PasswordChangedAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        IsEmailConfirmed = false;
        IsTwoFactorEnabled = false;
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
        UpdateTimestamp();
    }
    
    public void UpdateProfileImage(string profileImageUrl)
    {
        ProfileImageUrl = profileImageUrl ?? string.Empty;
        UpdateTimestamp();
    }
    
    public void UpdateCoverImage(string coverImageUrl)
    {
        CoverImageUrl = coverImageUrl ?? string.Empty;
        UpdateTimestamp();
    }
    
    public void SetPrivacyStatus(bool isPrivate)
    {
        IsPrivate = isPrivate;
        UpdateTimestamp();
    }
    
    public void SetVerificationStatus(bool isVerified)
    {
        IsVerified = isVerified;
        UpdateTimestamp();
    }
    
    public void SetBirthDate(DateTime birthDate)
    {
        if (birthDate > DateTime.UtcNow.AddYears(-13))
            throw new ArgumentException("User must be at least 13 years old");
            
        BirthDate = birthDate;
        UpdateTimestamp();
    }
    
    // Authentication methods
    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));
            
        PasswordHash = passwordHash;
        PasswordChangedAt = DateTime.UtcNow;
        FailedLoginAttempts = 0; // Reset failed attempts on successful password change
        UpdateTimestamp();
    }
    
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEndAt = null;
        UpdateTimestamp();
    }
    
    public void RecordFailedLogin(int maxFailedAttempts = 5, int lockoutMinutes = 30)
    {
        FailedLoginAttempts++;
        
        if (FailedLoginAttempts >= maxFailedAttempts)
        {
            LockoutEndAt = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
        
        UpdateTimestamp();
    }
    
    public bool IsLockedOut => LockoutEndAt.HasValue && LockoutEndAt.Value > DateTime.UtcNow;
    
    public void UnlockAccount()
    {
        FailedLoginAttempts = 0;
        LockoutEndAt = null;
        UpdateTimestamp();
    }
    
    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        EmailConfirmationToken = null;
        UpdateTimestamp();
    }
    
    public void SetEmailConfirmationToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required", nameof(token));
            
        EmailConfirmationToken = token;
        UpdateTimestamp();
    }
    
    public void EnableTwoFactor(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Two-factor secret is required", nameof(secret));
            
        IsTwoFactorEnabled = true;
        TwoFactorSecret = secret;
        UpdateTimestamp();
    }
    
    public void DisableTwoFactor()
    {
        IsTwoFactorEnabled = false;
        TwoFactorSecret = null;
        UpdateTimestamp();
    }
    
    // Methods expected by AuthService
    public void GenerateEmailConfirmationToken()
    {
        EmailConfirmationToken = Guid.NewGuid().ToString();
        UpdateTimestamp();
    }
    
    public void RecordFailedLoginAttempt()
    {
        RecordFailedLogin();
    }
    
    public void ChangePassword(string newPasswordHash)
    {
        UpdatePassword(newPasswordHash);
    }
    
    public void EnableTwoFactorAuthentication(string? secret = null)
    {
        var twoFactorSecret = secret ?? GenerateRandomSecret();
        EnableTwoFactor(twoFactorSecret);
    }
    
    public void DisableTwoFactorAuthentication()
    {
        DisableTwoFactor();
    }
    
    private string GenerateRandomSecret()
    {
        // Generate a 32-character base32 secret for TOTP
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 32)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
            
        Email = email.ToLowerInvariant();
        IsEmailConfirmed = false; // Require re-confirmation for new email
        EmailConfirmationToken = null;
        UpdateTimestamp();
    }
    
    private void UpdateTimestamp()
    {
        // Ensure UpdatedAt is always after the original timestamp
        var newTimestamp = DateTime.UtcNow;
        if (newTimestamp <= UpdatedAt)
            newTimestamp = UpdatedAt.AddTicks(1);
            
        UpdatedAt = newTimestamp;
    }
}
