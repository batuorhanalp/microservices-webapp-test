namespace WebApp.Common.Entities;

public class Follow
{
    public Guid Id { get; private set; }
    
    public Guid FollowerId { get; private set; }
    
    public Guid FolloweeId { get; private set; }
    
    public bool IsAccepted { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? AcceptedAt { get; private set; }

    // Navigation properties
    public User Follower { get; private set; } = null!;
    
    public User Followee { get; private set; } = null!;

    // Constructors
    private Follow() { } // For EF Core
    
    public Follow(Guid followerId, Guid followeeId, bool requiresApproval = false)
    {
        if (followerId == Guid.Empty)
            throw new ArgumentException("Follower ID is required", nameof(followerId));
            
        if (followeeId == Guid.Empty)
            throw new ArgumentException("Followee ID is required", nameof(followeeId));
            
        if (followerId == followeeId)
            throw new ArgumentException("User cannot follow themselves");

        Id = Guid.NewGuid();
        FollowerId = followerId;
        FolloweeId = followeeId;
        IsAccepted = !requiresApproval; // Auto-accept if no approval required
        CreatedAt = DateTime.UtcNow;
        
        if (IsAccepted)
            AcceptedAt = DateTime.UtcNow;
    }

    // Domain methods
    public void Accept()
    {
        if (IsAccepted)
            throw new InvalidOperationException("Follow request is already accepted");

        IsAccepted = true;
        AcceptedAt = DateTime.UtcNow;
    }
    
    public void Reject()
    {
        // This would typically be handled by deleting the record
        // But we can add a rejected state if needed for analytics
        throw new InvalidOperationException("To reject a follow request, delete the record");
    }
    
    public bool IsPending => !IsAccepted && !AcceptedAt.HasValue;
}
