using Microsoft.EntityFrameworkCore;
using WebApp.Core.Entities;

namespace WebApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<MediaAttachment> MediaAttachments { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Share> Shares { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(100);
            
            // Unique constraints
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Post Configuration
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasMaxLength(2000); // Twitter-like limit
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Visibility).HasConversion<int>();
            
            // Relationships
            entity.HasOne(p => p.Author)
                  .WithMany(u => u.Posts)
                  .HasForeignKey(p => p.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Self-referencing for replies
            entity.HasOne(p => p.ParentPost)
                  .WithMany(p => p.Replies)
                  .HasForeignKey(p => p.ParentPostId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(p => p.RootPost)
                  .WithMany()
                  .HasForeignKey(p => p.RootPostId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            // Indexes for performance
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ParentPostId);
        });

        // MediaAttachment Configuration
        modelBuilder.Entity<MediaAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AltText).HasMaxLength(200);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            
            // Relationship
            entity.HasOne(m => m.Post)
                  .WithMany(p => p.MediaAttachments)
                  .HasForeignKey(m => m.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.PostId);
        });

        // Follow Configuration
        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Relationships
            entity.HasOne(f => f.Follower)
                  .WithMany(u => u.Following)
                  .HasForeignKey(f => f.FollowerId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(f => f.Followee)
                  .WithMany(u => u.Followers)
                  .HasForeignKey(f => f.FolloweeId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Unique constraint to prevent duplicate follows
            entity.HasIndex(e => new { e.FollowerId, e.FolloweeId }).IsUnique();
        });

        // Like Configuration
        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Relationships
            entity.HasOne(l => l.User)
                  .WithMany(u => u.Likes)
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(l => l.Post)
                  .WithMany(p => p.Likes)
                  .HasForeignKey(l => l.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Unique constraint to prevent duplicate likes
            entity.HasIndex(e => new { e.UserId, e.PostId }).IsUnique();
        });

        // Comment Configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
            
            // Relationships
            entity.HasOne(c => c.User)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(c => c.Post)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(c => c.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.PostId);
        });

        // Share Configuration
        modelBuilder.Entity<Share>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(s => s.Post)
                  .WithMany(p => p.Shares)
                  .HasForeignKey(s => s.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Unique constraint to prevent duplicate shares
            entity.HasIndex(e => new { e.UserId, e.PostId }).IsUnique();
        });

        // Message Configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasMaxLength(2000);
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.AttachmentFileName).HasMaxLength(255);
            
            // Relationships
            entity.HasOne(m => m.Sender)
                  .WithMany(u => u.SentMessages)
                  .HasForeignKey(m => m.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(m => m.Recipient)
                  .WithMany(u => u.ReceivedMessages)
                  .HasForeignKey(m => m.RecipientId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            // Indexes for message queries
            entity.HasIndex(e => e.SenderId);
            entity.HasIndex(e => e.RecipientId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.SenderId, e.RecipientId, e.CreatedAt });
        });
    }
}
