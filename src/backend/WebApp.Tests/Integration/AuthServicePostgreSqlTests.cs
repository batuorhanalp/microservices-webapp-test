using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using Npgsql;

namespace WebApp.Tests.Integration;

/// <summary>
/// Real PostgreSQL database tests that exactly replicate the AuthDbContext configuration
/// These tests will reproduce the exact same errors that occur in the Auth service
/// </summary>
[Collection("PostgreSQL")]
public class AuthServicePostgreSqlTests : IDisposable
{
    private readonly string _connectionString;
    private readonly string _testDatabaseName;
    private TestAuthDbContext _context = null!;

    public AuthServicePostgreSqlTests()
    {
        // Generate unique test database name
        _testDatabaseName = $"auth_test_{Guid.NewGuid():N}";
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

        // Initialize DbContext with EXACT same configuration as AuthDbContext
        var options = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseNpgsql(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TestAuthDbContext(options);
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
    public void AuthService_Database_Migration_Should_Fail_With_RefreshToken_Backing_Field_Error()
    {
        // This test reproduces the EXACT error that occurs in the Auth service
        // It should FAIL and show us the RefreshToken.CreatedByIp backing field error
        
        var exception = Record.Exception(() =>
        {
            // This is the exact same call that fails in the Auth service Program.cs line 160
            _context.Database.Migrate();
        });
        
        // This test is EXPECTED to fail - it demonstrates the real bug
        exception.Should().NotBeNull("This test should fail and show the RefreshToken backing field error");
        exception.Should().BeOfType<InvalidOperationException>();
        exception!.Message.Should().Contain("No backing field could be found for property 'RefreshToken.CreatedByIp'");
        exception.Message.Should().Contain("and the property does not have a setter");
    }

    [Fact]
    public void AuthService_Database_EnsureCreated_Should_Fail_With_RefreshToken_Backing_Field_Error()
    {
        // Another way to trigger the same error
        
        var exception = Record.Exception(() =>
        {
            _context.Database.EnsureCreated();
        });
        
        // This test is EXPECTED to fail - it demonstrates the real bug
        exception.Should().NotBeNull("This test should fail and show the RefreshToken backing field error");
        exception.Should().BeOfType<InvalidOperationException>();
        exception!.Message.Should().Contain("No backing field could be found for property 'RefreshToken.CreatedByIp'");
    }

    [Fact]
    public void AuthService_RefreshToken_Entity_Creation_Should_Fail()
    {
        // Test specifically targeting RefreshToken entity creation
        
        var exception = Record.Exception(() =>
        {
            // This should fail during model creation/validation
            var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
            _context.Users.Add(user);
            
            var refreshToken = new RefreshToken(
                user.Id,
                "test-token",
                "jwt-id",
                DateTime.UtcNow.AddDays(7),
                "192.168.1.1",  // This property causes the backing field error
                "Test User Agent"
            );
            
            _context.RefreshTokens.Add(refreshToken);
            _context.SaveChanges(); // This should fail
        });
        
        // This test is EXPECTED to fail
        exception.Should().NotBeNull("RefreshToken creation should fail due to backing field error");
        exception.Should().BeOfType<InvalidOperationException>();
        exception!.Message.Should().Contain("RefreshToken.CreatedByIp");
    }

    [Fact]
    public void AuthService_Context_Model_Validation_Should_Fail()
    {
        // Test that accesses the model property to trigger validation
        
        var exception = Record.Exception(() =>
        {
            // Accessing the Model property triggers EF Core model validation
            var model = _context.Model;
            
            // This should trigger the validation error
            var refreshTokenEntity = model.FindEntityType(typeof(RefreshToken));
            refreshTokenEntity.Should().NotBeNull();
        });
        
        // This test is EXPECTED to fail
        exception.Should().NotBeNull("Model validation should fail due to RefreshToken backing field issues");
        exception.Should().BeOfType<InvalidOperationException>();
        exception!.Message.Should().Contain("RefreshToken.CreatedByIp");
    }
}

/// <summary>
/// Exact replica of the AuthDbContext from the Auth service
/// This WILL reproduce the RefreshToken backing field error
/// </summary>
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

        // Configure User entity - EXACT copy from AuthDbContext
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

            // Ignore Post navigation property to prevent Post relationship issues
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

        // Configure RefreshToken entity - EXACT copy from AuthDbContext
        // This configuration will CAUSE the backing field error
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
            
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(64);
            entity.Property(rt => rt.JwtId).IsRequired().HasMaxLength(64);
            entity.Property(rt => rt.CreatedByIp).HasMaxLength(45);  // THIS WILL CAUSE THE ERROR
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
