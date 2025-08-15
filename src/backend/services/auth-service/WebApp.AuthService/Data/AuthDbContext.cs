using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;

namespace WebApp.AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    // DbSets for auth-related entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
            
            entity.Property(u => u.Email).IsRequired().HasMaxLength(320);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(128);
            entity.Property(u => u.DisplayName).HasMaxLength(100);
            entity.Property(u => u.Bio).HasMaxLength(500);
            entity.Property(u => u.Location).HasMaxLength(100);
            entity.Property(u => u.Website).HasMaxLength(200);
            entity.Property(u => u.ProfileImageUrl).HasMaxLength(500);
            entity.Property(u => u.CoverImageUrl).HasMaxLength(500);
            entity.Property(u => u.EmailConfirmationToken).HasMaxLength(64);
            entity.Property(u => u.TwoFactorSecret).HasMaxLength(32);
            
            // Ignore Post navigation property to avoid Post relationship validation issues
            entity.Ignore(u => u.Posts);

            // Configure relationships
            entity.HasMany<RefreshToken>()
                  .WithOne(rt => rt.User)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<PasswordResetToken>()
                  .WithOne(prt => prt.User)
                  .HasForeignKey(prt => prt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<UserSession>()
                  .WithOne(us => us.User)
                  .HasForeignKey(us => us.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
            
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(64);
            entity.Property(rt => rt.JwtId).IsRequired().HasMaxLength(64);
            entity.Property(rt => rt.RevokedByIp).HasMaxLength(45);
            entity.Property(rt => rt.RevokedReason).HasMaxLength(200);
            entity.Property(rt => rt.ReplacedByToken).HasMaxLength(64);
            
            // Ignore computed properties without backing fields
            entity.Ignore(rt => rt.CreatedByIp);
            entity.Ignore(rt => rt.IsActive);
            entity.Ignore(rt => rt.IsExpired);
        });

        // Configure PasswordResetToken entity
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetTokens");
            entity.HasKey(prt => prt.Id);
            entity.HasIndex(prt => prt.Token).IsUnique();
            entity.HasIndex(prt => prt.UserId);
            
            entity.Property(prt => prt.Token).IsRequired().HasMaxLength(64);
            entity.Property(prt => prt.IpAddress).HasMaxLength(45);
            
            // Ignore computed properties without backing fields
            entity.Ignore(prt => prt.IsValid);
            entity.Ignore(prt => prt.IsExpired);
        });

        // Configure UserSession entity
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions");
            entity.HasKey(us => us.Id);
            entity.HasIndex(us => us.SessionId).IsUnique();
            entity.HasIndex(us => us.UserId);
            
            entity.Property(us => us.SessionId).IsRequired().HasMaxLength(64);
            entity.Property(us => us.IpAddress).HasMaxLength(45);
            entity.Property(us => us.UserAgent).HasMaxLength(500);
            entity.Property(us => us.DeviceInfo).HasMaxLength(200);
            entity.Property(us => us.Location).HasMaxLength(100);
            
            // Ignore computed properties without backing fields
            entity.Ignore(us => us.IsExpired);
            entity.Ignore(us => us.IsValidSession);
        });
    }
}
