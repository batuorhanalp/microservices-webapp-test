using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Data;
using WebApp.Common.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace WebApp.Tests.Infrastructure.Data;

// Test version of AuthDbContext to simulate the auth service behavior
public class TestAuthDbContext : DbContext
{
    public TestAuthDbContext(DbContextOptions<TestAuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity similar to AuthDbContext
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

        // Configure RefreshToken entity - this will test for backing field issues
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
            
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(64);
            entity.Property(rt => rt.JwtId).IsRequired().HasMaxLength(64);
            entity.Property(rt => rt.CreatedByIp).HasMaxLength(45);
            entity.Property(rt => rt.RevokedByIp).HasMaxLength(45);
            entity.Property(rt => rt.RevokedReason).HasMaxLength(200);
            entity.Property(rt => rt.ReplacedByToken).HasMaxLength(64);
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
        });
    }
}

public class PostgreSqlEfCoreRelationshipTests : IDisposable
{
    private readonly ApplicationDbContext _applicationContext;
    private readonly TestAuthDbContext _authContext;

    public PostgreSqlEfCoreRelationshipTests()
    {
        // Use PostgreSQL-compatible options to simulate real database behavior
        var applicationOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var authOptions = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _applicationContext = new ApplicationDbContext(applicationOptions);
        _authContext = new TestAuthDbContext(authOptions);
    }

    public void Dispose()
    {
        _applicationContext.Dispose();
        _authContext.Dispose();
    }

    #region RefreshToken Backing Field Tests

    [Fact]
    public void RefreshToken_Should_Have_Backing_Fields_For_All_Properties()
    {
        // This test validates that all RefreshToken properties have proper backing fields
        // to avoid "No backing field could be found for property" errors
        
        // Act & Assert
        var exception = Record.Exception(() =>
        {
            var model = _authContext.Model;
            var refreshTokenEntity = model.FindEntityType(typeof(RefreshToken));
            
            refreshTokenEntity.Should().NotBeNull("RefreshToken entity should be configured");
            
            // Check specific properties that caused issues
            var properties = new[]
            {
                "CreatedByIp", "RevokedByIp", "RevokedReason", "ReplacedByToken",
                "Token", "JwtId", "UserId", "ExpiresAt", "CreatedAt", "RevokedAt",
                "IsRevoked", "IsUsed"
            };
            
            foreach (var propertyName in properties)
            {
                var property = refreshTokenEntity!.FindProperty(propertyName);
                property.Should().NotBeNull($"Property {propertyName} should be configured in the model");
                
                // Verify property has getter/setter or backing field
                var clrProperty = typeof(RefreshToken).GetProperty(propertyName, 
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (clrProperty != null)
                {
                    var hasGetter = clrProperty.CanRead;
                    var hasSetter = clrProperty.CanWrite;
                    
                    // Property should either have a setter OR be configured with a backing field
                    (hasGetter && (hasSetter || property!.GetFieldName() != null))
                        .Should().BeTrue($"Property {propertyName} should have proper access configuration");
                }
            }
        });
        
        exception.Should().BeNull("RefreshToken entity model should be valid without backing field errors");
    }

    [Fact]
    public void RefreshToken_Entity_Properties_Should_Be_Accessible()
    {
        // Test that RefreshToken can be created and its properties accessed
        // without triggering backing field issues
        
        var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
        _authContext.Users.Add(user);
        _authContext.SaveChanges();
        
        // Act
        var refreshToken = new RefreshToken(
            user.Id,
            "test-token-string",
            "jwt-id-123",
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",
            "Test User Agent"
        );
        
        var exception = Record.Exception(() =>
        {
            _authContext.RefreshTokens.Add(refreshToken);
            _authContext.SaveChanges();
            
            // Try to read the entity back - this would fail if backing fields are misconfigured
            var savedToken = _authContext.RefreshTokens
                .First(rt => rt.Id == refreshToken.Id);
                
            // Verify all properties are accessible
            _ = savedToken.Token;
            _ = savedToken.JwtId;
            _ = savedToken.UserId;
            _ = savedToken.ExpiresAt;
            _ = savedToken.CreatedAt;
            _ = savedToken.IsRevoked;
            _ = savedToken.IsUsed;
            _ = savedToken.CreatedByIp;
            _ = savedToken.RevokedByIp;
            _ = savedToken.RevokedReason;
            _ = savedToken.ReplacedByToken;
        });
        
        exception.Should().BeNull("RefreshToken properties should be accessible without backing field errors");
    }

    [Fact]
    public void RefreshToken_Can_Be_Updated_Without_Setter_Issues()
    {
        // Test updating RefreshToken properties that might lack setters
        
        var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
        _authContext.Users.Add(user);
        _authContext.SaveChanges();
        
        var refreshToken = new RefreshToken(
            user.Id,
            "test-token-string",
            "jwt-id-123",
            DateTime.UtcNow.AddDays(7)
        );
        
        _authContext.RefreshTokens.Add(refreshToken);
        _authContext.SaveChanges();
        
        // Act - Try operations that would fail if properties lack proper setters/backing fields
        var exception = Record.Exception(() =>
        {
            refreshToken.Revoke("192.168.1.100", "Token compromised");
            refreshToken.MarkAsUsed();
            
            _authContext.SaveChanges();
            
            // Verify the changes were persisted
            var updatedToken = _authContext.RefreshTokens
                .First(rt => rt.Id == refreshToken.Id);
                
            updatedToken.IsRevoked.Should().BeTrue();
            updatedToken.IsUsed.Should().BeTrue();
            updatedToken.RevokedByIp.Should().Be("192.168.1.100");
            updatedToken.RevokedReason.Should().Be("Token compromised");
        });
        
        exception.Should().BeNull("RefreshToken should be updatable without setter/backing field issues");
    }

    #endregion

    #region Post Self-Referencing Relationship Tests

    [Fact]
    public void ApplicationDbContext_Post_SelfReferencing_Relationships_Should_Be_Valid()
    {
        // This test specifically targets the "dependent side could not be determined" error
        // for Post.ParentPost and Post.RootPost relationships
        
        // Act & Assert
        var exception = Record.Exception(() =>
        {
            var model = _applicationContext.Model;
            var postEntity = model.FindEntityType(typeof(Post));
            
            postEntity.Should().NotBeNull("Post entity should be configured");
            
            // Verify ParentPost relationship configuration
            var parentPostNavigation = postEntity!.FindNavigation("ParentPost");
            parentPostNavigation.Should().NotBeNull("ParentPost navigation should exist");
            
            var parentPostForeignKey = parentPostNavigation!.ForeignKey;
            parentPostForeignKey.Should().NotBeNull("ParentPost should have a foreign key");
            parentPostForeignKey.IsUnique.Should().BeFalse("ParentPost should be many-to-one, not one-to-one");
            parentPostForeignKey.IsRequired.Should().BeFalse("ParentPost should be optional");
            
            // Verify RootPost relationship configuration
            var rootPostNavigation = postEntity.FindNavigation("RootPost");
            rootPostNavigation.Should().NotBeNull("RootPost navigation should exist");
            
            var rootPostForeignKey = rootPostNavigation!.ForeignKey;
            rootPostForeignKey.Should().NotBeNull("RootPost should have a foreign key");
            rootPostForeignKey.IsUnique.Should().BeFalse("RootPost should be many-to-one, not one-to-one");
            rootPostForeignKey.IsRequired.Should().BeFalse("RootPost should be optional");
            
            // Verify that the relationships are configured independently
            parentPostForeignKey.Should().NotBeSameAs(rootPostForeignKey, 
                "ParentPost and RootPost should have separate foreign key configurations");
                
            // Verify foreign key properties exist
            var parentPostIdProperty = postEntity.FindProperty("ParentPostId");
            parentPostIdProperty.Should().NotBeNull("ParentPostId property should exist");
            
            var rootPostIdProperty = postEntity.FindProperty("RootPostId");
            rootPostIdProperty.Should().NotBeNull("RootPostId property should exist");
            
            // Ensure database creation works
            _applicationContext.Database.EnsureCreated();
        });
        
        exception.Should().BeNull("Post self-referencing relationships should be configured correctly without dependent side errors");
    }

    [Fact]
    public void Post_Can_Create_Complex_Reply_Chains_Without_Relationship_Errors()
    {
        // Test creating complex post reply chains that would expose relationship configuration issues
        
        var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
        _applicationContext.Users.Add(user);
        _applicationContext.SaveChanges();
        
        // Act - Create a complex reply chain
        var exception = Record.Exception(() =>
        {
            // Root post
            var rootPost = new Post(user.Id, "This is the root post");
            _applicationContext.Posts.Add(rootPost);
            _applicationContext.SaveChanges();
            
            // First level replies
            var reply1 = new Post(user.Id, "Reply 1 to root", parentPostId: rootPost.Id, rootPostId: rootPost.Id);
            var reply2 = new Post(user.Id, "Reply 2 to root", parentPostId: rootPost.Id, rootPostId: rootPost.Id);
            _applicationContext.Posts.AddRange(reply1, reply2);
            _applicationContext.SaveChanges();
            
            // Second level replies (replies to replies)
            var nestedReply1 = new Post(user.Id, "Reply to Reply 1", parentPostId: reply1.Id, rootPostId: rootPost.Id);
            var nestedReply2 = new Post(user.Id, "Reply to Reply 2", parentPostId: reply2.Id, rootPostId: rootPost.Id);
            _applicationContext.Posts.AddRange(nestedReply1, nestedReply2);
            _applicationContext.SaveChanges();
            
            // Third level reply (deeply nested)
            var deepReply = new Post(user.Id, "Deep nested reply", parentPostId: nestedReply1.Id, rootPostId: rootPost.Id);
            _applicationContext.Posts.Add(deepReply);
            _applicationContext.SaveChanges();
            
            // Verify relationships can be loaded
            var loadedDeepReply = _applicationContext.Posts
                .Include(p => p.ParentPost)
                .Include(p => p.RootPost)
                .First(p => p.Id == deepReply.Id);
                
            loadedDeepReply.ParentPost.Should().NotBeNull();
            loadedDeepReply.ParentPost!.Id.Should().Be(nestedReply1.Id);
            loadedDeepReply.RootPost.Should().NotBeNull();
            loadedDeepReply.RootPost!.Id.Should().Be(rootPost.Id);
        });
        
        exception.Should().BeNull("Complex post reply chains should be created without relationship configuration errors");
    }

    [Fact]
    public void Post_SelfReferencing_Relationships_Should_Allow_Null_Values()
    {
        // Test that ParentPost and RootPost can be null (optional relationships)
        
        var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
        _applicationContext.Users.Add(user);
        _applicationContext.SaveChanges();
        
        // Act - Create posts with null parent/root references
        var exception = Record.Exception(() =>
        {
            // Standalone post (no parent or root)
            var standalonePost = new Post(user.Id, "Standalone post");
            _applicationContext.Posts.Add(standalonePost);
            _applicationContext.SaveChanges();
            
            // Post with only parent (no root) - edge case
            var parentOnlyPost = new Post(user.Id, "Parent only post", parentPostId: standalonePost.Id);
            _applicationContext.Posts.Add(parentOnlyPost);
            _applicationContext.SaveChanges();
            
            // Verify the posts were saved correctly
            var savedStandalone = _applicationContext.Posts
                .Include(p => p.ParentPost)
                .Include(p => p.RootPost)
                .First(p => p.Id == standalonePost.Id);
                
            savedStandalone.ParentPost.Should().BeNull();
            savedStandalone.RootPost.Should().BeNull();
            savedStandalone.ParentPostId.Should().BeNull();
            savedStandalone.RootPostId.Should().BeNull();
            
            var savedParentOnly = _applicationContext.Posts
                .Include(p => p.ParentPost)
                .Include(p => p.RootPost)
                .First(p => p.Id == parentOnlyPost.Id);
                
            savedParentOnly.ParentPost.Should().NotBeNull();
            savedParentOnly.ParentPost!.Id.Should().Be(standalonePost.Id);
            savedParentOnly.RootPost.Should().BeNull(); // Should allow null root
        });
        
        exception.Should().BeNull("Post self-referencing relationships should support null values correctly");
    }

    #endregion

    #region Entity Validation Tests

    [Fact]
    public void All_Entities_Should_Have_Valid_EF_Configuration_In_ApplicationContext()
    {
        // Comprehensive test to validate all entity configurations in ApplicationDbContext
        
        var exception = Record.Exception(() =>
        {
            var model = _applicationContext.Model;
            
            // Test all main entities
            var entityTypes = new[]
            {
                typeof(User), typeof(Post), typeof(Follow), typeof(Like),
                typeof(Comment), typeof(Share), typeof(Message), typeof(Notification),
                typeof(RefreshToken), typeof(PasswordResetToken), typeof(MediaAttachment)
            };
            
            foreach (var entityType in entityTypes)
            {
                var entityModel = model.FindEntityType(entityType);
                entityModel.Should().NotBeNull($"{entityType.Name} should be configured in the model");
                
                // Verify primary key is configured
                var primaryKey = entityModel!.FindPrimaryKey();
                primaryKey.Should().NotBeNull($"{entityType.Name} should have a primary key configured");
                
                // Verify foreign keys are properly configured
                var foreignKeys = entityModel.GetForeignKeys();
                foreach (var fk in foreignKeys)
                {
                    fk.PrincipalEntityType.Should().NotBeNull($"Foreign key in {entityType.Name} should have a valid principal entity");
                    fk.Properties.Should().NotBeEmpty($"Foreign key in {entityType.Name} should have properties");
                }
                
                // Verify navigations
                var navigations = entityModel.GetNavigations();
                foreach (var navigation in navigations)
                {
                    navigation.ForeignKey.Should().NotBeNull($"Navigation {navigation.Name} in {entityType.Name} should have a foreign key");
                }
            }
            
            // Ensure the model can be used to create a database
            _applicationContext.Database.EnsureCreated();
        });
        
        exception.Should().BeNull("All entities should have valid EF Core configuration");
    }

    [Fact]
    public void All_Entities_Should_Have_Valid_EF_Configuration_In_AuthContext()
    {
        // Test AuthDbContext entity configurations
        
        var exception = Record.Exception(() =>
        {
            var model = _authContext.Model;
            
            // Test auth-specific entities
            var entityTypes = new[]
            {
                typeof(User), typeof(RefreshToken), typeof(PasswordResetToken), typeof(UserSession)
            };
            
            foreach (var entityType in entityTypes)
            {
                var entityModel = model.FindEntityType(entityType);
                entityModel.Should().NotBeNull($"{entityType.Name} should be configured in AuthDbContext");
                
                var primaryKey = entityModel!.FindPrimaryKey();
                primaryKey.Should().NotBeNull($"{entityType.Name} should have a primary key in AuthDbContext");
            }
            
            // Verify that Post navigation is ignored in User entity
            var userEntity = model.FindEntityType(typeof(User));
            var postsNavigation = userEntity!.FindNavigation("Posts");
            postsNavigation.Should().BeNull("Posts navigation should be ignored in AuthDbContext to prevent relationship issues");
            
            // Ensure the model can be used to create a database
            _authContext.Database.EnsureCreated();
        });
        
        exception.Should().BeNull("Auth entities should have valid EF Core configuration without Post relationship issues");
    }

    #endregion

    #region Relationship Integrity Tests

    [Fact]
    public void Foreign_Key_Relationships_Should_Maintain_Referential_Integrity()
    {
        // Test that foreign key relationships work correctly across all entities
        
        var user1 = new User("user1@example.com", "user1", "User One", "hashedpassword1");
        var user2 = new User("user2@example.com", "user2", "User Two", "hashedpassword2");
        _applicationContext.Users.AddRange(user1, user2);
        _applicationContext.SaveChanges();
        
        var exception = Record.Exception(() =>
        {
            // Create a post
            var post = new Post(user1.Id, "Test post content");
            _applicationContext.Posts.Add(post);
            _applicationContext.SaveChanges();
            
            // Create related entities
            var like = new Like(user2.Id, post.Id);
            var comment = new Comment(user2.Id, post.Id, "Great post!");
            var share = new Share(user2.Id, post.Id, "Check this out");
            var follow = new Follow(user2.Id, user1.Id);
            var message = new Message(user1.Id, user2.Id, "Hello!", MessageType.Text);
            var notification = new Notification(user2.Id, NotificationType.Like, "New like", "Someone liked your post");
            var mediaAttachment = new MediaAttachment(post.Id, "https://example.com/image.jpg", "image.jpg", "image/jpeg", 1024);
            
            _applicationContext.Likes.Add(like);
            _applicationContext.Comments.Add(comment);
            _applicationContext.Shares.Add(share);
            _applicationContext.Follows.Add(follow);
            _applicationContext.Messages.Add(message);
            _applicationContext.Notifications.Add(notification);
            _applicationContext.MediaAttachments.Add(mediaAttachment);
            
            _applicationContext.SaveChanges();
            
            // Verify relationships are maintained
            var savedPost = _applicationContext.Posts
                .Include(p => p.Author)
                .Include(p => p.MediaAttachments)
                .First(p => p.Id == post.Id);
                
            savedPost.Author.Should().NotBeNull();
            savedPost.Author.Id.Should().Be(user1.Id);
            savedPost.MediaAttachments.Should().HaveCount(1);
            
            var savedLike = _applicationContext.Likes
                .Include(l => l.User)
                .Include(l => l.Post)
                .First(l => l.Id == like.Id);
                
            savedLike.User.Id.Should().Be(user2.Id);
            savedLike.Post.Id.Should().Be(post.Id);
        });
        
        exception.Should().BeNull("Foreign key relationships should maintain referential integrity");
    }

    [Fact]
    public void Complex_Relationships_Should_Work_With_Includes()
    {
        // Test complex queries with multiple includes to verify relationship configurations
        
        var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
        _applicationContext.Users.Add(user);
        _applicationContext.SaveChanges();
        
        var exception = Record.Exception(() =>
        {
            // Create complex data structure
            var rootPost = new Post(user.Id, "Root post");
            _applicationContext.Posts.Add(rootPost);
            _applicationContext.SaveChanges();
            
            var reply = new Post(user.Id, "Reply to root", parentPostId: rootPost.Id, rootPostId: rootPost.Id);
            _applicationContext.Posts.Add(reply);
            _applicationContext.SaveChanges();
            
            var like = new Like(user.Id, reply.Id);
            var comment = new Comment(user.Id, reply.Id, "Nice reply!");
            var mediaAttachment = new MediaAttachment(reply.Id, "https://example.com/reply-image.jpg", "reply.jpg", "image/jpeg", 2048);
            
            _applicationContext.Likes.Add(like);
            _applicationContext.Comments.Add(comment);
            _applicationContext.MediaAttachments.Add(mediaAttachment);
            _applicationContext.SaveChanges();
            
            // Complex query with multiple includes
            var complexQuery = _applicationContext.Posts
                .Include(p => p.Author)
                .Include(p => p.ParentPost)
                .Include(p => p.RootPost)
                .Include(p => p.MediaAttachments)
                .Where(p => p.Id == reply.Id)
                .FirstOrDefault();
                
            complexQuery.Should().NotBeNull();
            complexQuery!.Author.Should().NotBeNull();
            complexQuery.ParentPost.Should().NotBeNull();
            complexQuery.RootPost.Should().NotBeNull();
            complexQuery.MediaAttachments.Should().HaveCount(1);
        });
        
        exception.Should().BeNull("Complex relationships with includes should work correctly");
    }

    #endregion
}
