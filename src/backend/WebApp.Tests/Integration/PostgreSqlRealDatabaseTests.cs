using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Data;
using WebApp.Common.Entities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace WebApp.Tests.Integration;

/// <summary>
/// Real PostgreSQL database tests that reproduce actual production issues
/// These tests use real PostgreSQL connections to catch EF Core validation errors
/// </summary>
[Collection("PostgreSQL")]
public class PostgreSqlRealDatabaseTests : IDisposable
{
    private readonly string _connectionString;
    private readonly string _testDatabaseName;
    private ApplicationDbContext _context = null!;

    public PostgreSqlRealDatabaseTests()
    {
        // Generate unique test database name
        _testDatabaseName = $"webapp_test_{Guid.NewGuid():N}";
        _connectionString = $"Host=localhost;Port=5432;Database={_testDatabaseName};Username=webapp_user;Password=webapp_dev_password";
        
        InitializeTestDatabase();
    }

    private void InitializeTestDatabase()
    {
        // Create test database
        var masterConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=webapp_user;Password=webapp_dev_password";
        
        try
        {
            using var masterConnection = new NpgsqlConnection(masterConnectionString);
            masterConnection.Open();
            
            using var createDbCommand = masterConnection.CreateCommand();
            createDbCommand.CommandText = $"CREATE DATABASE \"{_testDatabaseName}\"";
            createDbCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create test database. Ensure PostgreSQL is running and credentials are correct: {ex.Message}", ex);
        }

        // Initialize DbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _context?.Dispose();
        
        // Clean up test database
        try
        {
            var masterConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=webapp_user;Password=webapp_dev_password";
            using var masterConnection = new NpgsqlConnection(masterConnectionString);
            masterConnection.Open();
            
            // Force close connections to test database
            using var killConnectionsCommand = masterConnection.CreateCommand();
            killConnectionsCommand.CommandText = $@"
                SELECT pg_terminate_backend(pid) 
                FROM pg_stat_activity 
                WHERE datname = '{_testDatabaseName}' AND pid <> pg_backend_pid()";
            killConnectionsCommand.ExecuteNonQuery();
            
            // Drop test database
            using var dropDbCommand = masterConnection.CreateCommand();
            dropDbCommand.CommandText = $"DROP DATABASE IF EXISTS \"{_testDatabaseName}\"";
            dropDbCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            // Log but don't fail test cleanup
            Console.WriteLine($"Warning: Failed to cleanup test database {_testDatabaseName}: {ex.Message}");
        }
    }

    [Fact]
    public void PostgreSQL_EF_Model_Should_Pass_Validation_Without_Errors()
    {
        // This test will fail with the exact same errors as the Auth service
        // because it uses real PostgreSQL validation
        
        var exception = Record.Exception(() =>
        {
            // This will trigger EF Core model validation against PostgreSQL
            // and reproduce the exact errors we see in production
            _context.Database.EnsureCreated();
        });
        
        // If this passes, the model is valid. If it fails, we'll see the real errors.
        exception.Should().BeNull("EF Core model should be valid for PostgreSQL without validation errors");
    }

    [Fact]
    public void PostgreSQL_RefreshToken_Should_Be_Creatable_Without_Backing_Field_Errors()
    {
        // This test will reproduce the RefreshToken backing field error
        
        var exception = Record.Exception(() =>
        {
            // Try to ensure database is created - this should fail with backing field error
            _context.Database.EnsureCreated();
            
            // Create test user
            var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
            _context.Users.Add(user);
            _context.SaveChanges();
            
            // Try to create RefreshToken - this will expose backing field issues
            var refreshToken = new RefreshToken(
                user.Id,
                "test-token-string",
                "jwt-id-123",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1",  // This should trigger the CreatedByIp backing field error
                "Test User Agent"
            );
            
            _context.RefreshTokens.Add(refreshToken);
            _context.SaveChanges();
        });
        
        exception.Should().BeNull("RefreshToken should be creatable without backing field errors");
    }

    [Fact]
    public void PostgreSQL_Post_SelfReferencing_Relationships_Should_Work()
    {
        // This test will reproduce the Post self-referencing relationship errors
        
        var exception = Record.Exception(() =>
        {
            // Try to create database schema - this should expose relationship configuration errors
            _context.Database.EnsureCreated();
            
            // Create test user
            var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
            _context.Users.Add(user);
            _context.SaveChanges();
            
            // Create root post
            var rootPost = new Post(user.Id, "Root post content");
            _context.Posts.Add(rootPost);
            _context.SaveChanges();
            
            // Create reply post - this should trigger self-referencing relationship issues
            var replyPost = new Post(user.Id, "Reply content", parentPostId: rootPost.Id, rootPostId: rootPost.Id);
            _context.Posts.Add(replyPost);
            _context.SaveChanges();
            
            // Try to load the post with relationships
            var loadedPost = _context.Posts
                .Include(p => p.ParentPost)
                .Include(p => p.RootPost)
                .First(p => p.Id == replyPost.Id);
                
            loadedPost.ParentPost.Should().NotBeNull();
            loadedPost.RootPost.Should().NotBeNull();
        });
        
        exception.Should().BeNull("Post self-referencing relationships should work without configuration errors");
    }

    [Fact]
    public void PostgreSQL_Database_Migration_Should_Succeed()
    {
        // This test reproduces the exact migration error from the Auth service
        
        var exception = Record.Exception(() =>
        {
            // This is the exact same call that fails in the Auth service Program.cs
            _context.Database.Migrate();
        });
        
        exception.Should().BeNull("Database migration should succeed without errors");
    }

    [Fact]
    public void PostgreSQL_All_Entity_Relationships_Should_Be_Valid()
    {
        // Comprehensive test that creates all entity types to expose relationship issues
        
        var exception = Record.Exception(() =>
        {
            _context.Database.EnsureCreated();
            
            // Create users
            var user1 = new User("user1@example.com", "user1", "User One", "hashedpassword1");
            var user2 = new User("user2@example.com", "user2", "User Two", "hashedpassword2");
            _context.Users.AddRange(user1, user2);
            _context.SaveChanges();
            
            // Create post with all relationships
            var post = new Post(user1.Id, "Test post content");
            _context.Posts.Add(post);
            _context.SaveChanges();
            
            // Create all related entities that could have relationship issues
            var like = new Like(user2.Id, post.Id);
            var comment = new Comment(user2.Id, post.Id, "Test comment");
            var share = new Share(user2.Id, post.Id, "Test share");
            var follow = new Follow(user2.Id, user1.Id);
            var message = new Message(user1.Id, user2.Id, "Test message", MessageType.Text);
            var notification = new Notification(user2.Id, NotificationType.Like, "Test notification", "Test message");
            var mediaAttachment = new MediaAttachment(post.Id, "https://example.com/test.jpg", "test.jpg", "image/jpeg", 1024);
            
            // Try to create refresh token that has backing field issues
            var refreshToken = new RefreshToken(user1.Id, "test-token", "jwt-id", DateTime.UtcNow.AddDays(7), "192.168.1.1", "User Agent");
            var passwordResetToken = new PasswordResetToken(user1.Id, "reset-token", DateTime.UtcNow.AddHours(1), "192.168.1.1");
            
            _context.Likes.Add(like);
            _context.Comments.Add(comment);
            _context.Shares.Add(share);
            _context.Follows.Add(follow);
            _context.Messages.Add(message);
            _context.Notifications.Add(notification);
            _context.MediaAttachments.Add(mediaAttachment);
            _context.RefreshTokens.Add(refreshToken);
            _context.PasswordResetTokens.Add(passwordResetToken);
            
            _context.SaveChanges();
            
            // Verify all entities were created successfully
            _context.Posts.Should().HaveCount(1);
            _context.Likes.Should().HaveCount(1);
            _context.Comments.Should().HaveCount(1);
            _context.Shares.Should().HaveCount(1);
            _context.Follows.Should().HaveCount(1);
            _context.Messages.Should().HaveCount(1);
            _context.Notifications.Should().HaveCount(1);
            _context.MediaAttachments.Should().HaveCount(1);
            _context.RefreshTokens.Should().HaveCount(1);
            _context.PasswordResetTokens.Should().HaveCount(1);
        });
        
        exception.Should().BeNull("All entity relationships should work without errors");
    }

    [Fact]
    public void PostgreSQL_Complex_Query_With_Includes_Should_Work()
    {
        // Test complex queries that would expose relationship configuration issues
        
        var exception = Record.Exception(() =>
        {
            _context.Database.EnsureCreated();
            
            var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
            _context.Users.Add(user);
            _context.SaveChanges();
            
            // Create complex post hierarchy
            var rootPost = new Post(user.Id, "Root post");
            _context.Posts.Add(rootPost);
            _context.SaveChanges();
            
            var reply1 = new Post(user.Id, "Reply 1", parentPostId: rootPost.Id, rootPostId: rootPost.Id);
            var reply2 = new Post(user.Id, "Reply 2", parentPostId: rootPost.Id, rootPostId: rootPost.Id);
            _context.Posts.AddRange(reply1, reply2);
            _context.SaveChanges();
            
            var nestedReply = new Post(user.Id, "Nested reply", parentPostId: reply1.Id, rootPostId: rootPost.Id);
            _context.Posts.Add(nestedReply);
            _context.SaveChanges();
            
            // Add media and interactions
            var mediaAttachment = new MediaAttachment(nestedReply.Id, "https://example.com/image.jpg", "image.jpg", "image/jpeg", 1024);
            var like = new Like(user.Id, nestedReply.Id);
            var comment = new Comment(user.Id, nestedReply.Id, "Great nested reply!");
            
            _context.MediaAttachments.Add(mediaAttachment);
            _context.Likes.Add(like);
            _context.Comments.Add(comment);
            _context.SaveChanges();
            
            // Complex query with multiple includes - this would fail if relationships are misconfigured
            var complexResult = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.ParentPost)
                    .ThenInclude(pp => pp!.Author)
                .Include(p => p.RootPost)
                    .ThenInclude(rp => rp!.Author)
                .Include(p => p.MediaAttachments)
                .Where(p => p.Id == nestedReply.Id)
                .FirstOrDefault();
                
            complexResult.Should().NotBeNull();
            complexResult!.ParentPost.Should().NotBeNull();
            complexResult.RootPost.Should().NotBeNull();
            complexResult.MediaAttachments.Should().HaveCount(1);
        });
        
        exception.Should().BeNull("Complex queries with includes should work without relationship errors");
    }

    [Fact]
    public void PostgreSQL_RefreshToken_Properties_Should_Be_Accessible()
    {
        // Test specifically for RefreshToken backing field issues
        _context.Database.EnsureCreated();
        
        var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
        _context.Users.Add(user);
        _context.SaveChanges();
        
        // Create RefreshToken with all properties that might have backing field issues
        var refreshToken = new RefreshToken(
            user.Id,
            "test-token-string",
            "jwt-id-123",
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",  // CreatedByIp - this property causes the backing field error
            "Mozilla/5.0 Test User Agent"
        );
        
        _context.RefreshTokens.Add(refreshToken);
        _context.SaveChanges();
        
        // Test property access - this will fail if backing fields are not configured
        // Enable tracking for this query since we need to modify the entity
        var savedToken = _context.RefreshTokens.AsTracking().First(rt => rt.Id == refreshToken.Id);
        
        // Access all properties that might have backing field issues
        var createdByIp = savedToken.CreatedByIp; // This line should trigger the backing field error
        var token = savedToken.Token;
        var jwtId = savedToken.JwtId;
        var userId = savedToken.UserId;
        var expiresAt = savedToken.ExpiresAt;
        var createdAt = savedToken.CreatedAt;
        var isRevoked = savedToken.IsRevoked;
        var isUsed = savedToken.IsUsed;
        
        // Before revocation, IsRevoked should be false
        savedToken.IsRevoked.Should().BeFalse("Token should not be revoked initially");
        
        // Test mutation operations
        savedToken.Revoke("192.168.1.100", "Test revocation");
        
        // Check immediately after calling Revoke (before SaveChanges)
        savedToken.IsRevoked.Should().BeTrue("Token should be revoked after calling Revoke method");
        
        // Check EF Core change tracking state
        var entityEntry = _context.Entry(savedToken);
        var entityState = entityEntry.State;
        Console.WriteLine($"Entity State after Revoke(): {entityState}");
        
        // Check which properties EF Core thinks have changed
        var modifiedProperties = entityEntry.Properties
            .Where(p => p.IsModified)
            .Select(p => $"{p.Metadata.Name}: {p.OriginalValue} -> {p.CurrentValue}")
            .ToList();
        
        Console.WriteLine($"Modified properties: {string.Join(", ", modifiedProperties)}");
        
        entityState.Should().NotBe(EntityState.Unchanged, "Entity should be detected as modified after Revoke call");
        
        // Explicitly mark entity as modified if EF Core didn't detect changes
        if (entityEntry.State == EntityState.Unchanged)
        {
            Console.WriteLine("WARNING: EF Core didn't detect changes, manually marking as modified");
            _context.Entry(savedToken).State = EntityState.Modified;
        }
        
        // Save and capture any SQL generated
        Console.WriteLine("Saving changes...");
        var changesSaved = _context.SaveChanges();
        Console.WriteLine($"Number of changes saved: {changesSaved}");
        
        // Clear the context to force reload from database
        _context.Entry(savedToken).Reload();
        
        // Check the values after reload
        Console.WriteLine($"After reload - IsRevoked: {savedToken.IsRevoked}, RevokedByIp: {savedToken.RevokedByIp}, RevokedReason: {savedToken.RevokedReason}");
        
        // Also get a fresh instance from database
        var revokedToken = _context.RefreshTokens.AsNoTracking().First(rt => rt.Id == refreshToken.Id);
        Console.WriteLine($"Fresh from DB - IsRevoked: {revokedToken.IsRevoked}, RevokedByIp: {revokedToken.RevokedByIp}, RevokedReason: {revokedToken.RevokedReason}");
        
        revokedToken.IsRevoked.Should().BeTrue("Token should remain revoked after save");
        revokedToken.RevokedByIp.Should().Be("192.168.1.100");
        revokedToken.RevokedReason.Should().Be("Test revocation");
    }

    [Fact]
    public void PostgreSQL_Database_Schema_Creation_Should_Handle_All_Constraints()
    {
        // Test that schema creation handles all unique constraints and indexes properly
        
        var exception = Record.Exception(() =>
        {
            _context.Database.EnsureCreated();
            
            // Test unique constraints
            var user1 = new User("unique@example.com", "uniqueuser", "User One", "hashedpassword");
            _context.Users.Add(user1);
            _context.SaveChanges();
            
            // This should fail due to unique constraint on email
            var duplicateEmailUser = new User("unique@example.com", "differentusername", "User Two", "hashedpassword");
            
            var duplicateEmailException = Record.Exception(() =>
            {
                _context.Users.Add(duplicateEmailUser);
                _context.SaveChanges();
            });
            
            duplicateEmailException.Should().NotBeNull("Duplicate email should be prevented by unique constraint");
            duplicateEmailException.Should().BeOfType<DbUpdateException>();
            
            // Clear the context to reset the tracking
            _context.ChangeTracker.Clear();
            
            // This should also fail due to unique constraint on username
            var duplicateUsernameUser = new User("different@example.com", "uniqueuser", "User Three", "hashedpassword");
            
            var duplicateUsernameException = Record.Exception(() =>
            {
                _context.Users.Add(duplicateUsernameUser);
                _context.SaveChanges();
            });
            
            duplicateUsernameException.Should().NotBeNull("Duplicate username should be prevented by unique constraint");
            duplicateUsernameException.Should().BeOfType<DbUpdateException>();
        });
        
        if (exception != null)
        {
            throw exception;
        }
    }
}

/// <summary>
/// Test collection to ensure PostgreSQL tests run sequentially to avoid database conflicts
/// </summary>
[CollectionDefinition("PostgreSQL")]
public class PostgreSqlTestCollection : ICollectionFixture<PostgreSqlTestCollection>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
