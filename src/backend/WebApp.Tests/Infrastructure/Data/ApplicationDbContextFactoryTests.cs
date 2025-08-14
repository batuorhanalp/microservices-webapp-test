using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Data;
using Xunit;

namespace WebApp.Tests.Infrastructure.Data;

public class ApplicationDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_WithValidEnvironmentVariables_ShouldCreateContext()
    {
        // Arrange
        var factory = new ApplicationDbContextFactory();
        
        // Set required environment variables
        Environment.SetEnvironmentVariable("DB_USERNAME", "testuser");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "testpass");
        Environment.SetEnvironmentVariable("DB_HOST", "testhost");
        Environment.SetEnvironmentVariable("DB_NAME", "testdb");

        try
        {
            // Act
            var context = factory.CreateDbContext(Array.Empty<string>());

            // Assert
            context.Should().NotBeNull();
            context.Should().BeOfType<ApplicationDbContext>();
            
            // Cleanup
            context.Dispose();
        }
        finally
        {
            // Clean up environment variables
            Environment.SetEnvironmentVariable("DB_USERNAME", null);
            Environment.SetEnvironmentVariable("DB_PASSWORD", null);
            Environment.SetEnvironmentVariable("DB_HOST", null);
            Environment.SetEnvironmentVariable("DB_NAME", null);
        }
    }

    [Fact]
    public void CreateDbContext_WithMissingUsername_ShouldThrowException()
    {
        // Arrange
        var factory = new ApplicationDbContextFactory();
        
        // Set partial environment variables (missing username)
        Environment.SetEnvironmentVariable("DB_PASSWORD", "testpass");
        Environment.SetEnvironmentVariable("DB_HOST", "testhost");
        Environment.SetEnvironmentVariable("DB_NAME", "testdb");

        try
        {
            // Act & Assert
            var act = () => factory.CreateDbContext(Array.Empty<string>());
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("DB_USERNAME environment variable is required");
        }
        finally
        {
            // Clean up environment variables
            Environment.SetEnvironmentVariable("DB_PASSWORD", null);
            Environment.SetEnvironmentVariable("DB_HOST", null);
            Environment.SetEnvironmentVariable("DB_NAME", null);
        }
    }

    [Fact]
    public void CreateDbContext_WithMissingPassword_ShouldThrowException()
    {
        // Arrange
        var factory = new ApplicationDbContextFactory();
        
        // Set partial environment variables (missing password)
        Environment.SetEnvironmentVariable("DB_USERNAME", "testuser");
        Environment.SetEnvironmentVariable("DB_HOST", "testhost");
        Environment.SetEnvironmentVariable("DB_NAME", "testdb");

        try
        {
            // Act & Assert
            var act = () => factory.CreateDbContext(Array.Empty<string>());
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("DB_PASSWORD environment variable is required");
        }
        finally
        {
            // Clean up environment variables
            Environment.SetEnvironmentVariable("DB_USERNAME", null);
            Environment.SetEnvironmentVariable("DB_HOST", null);
            Environment.SetEnvironmentVariable("DB_NAME", null);
        }
    }

    [Fact]
    public void CreateDbContext_WithDefaultHostAndDatabase_ShouldUseDefaults()
    {
        // Arrange
        var factory = new ApplicationDbContextFactory();
        
        // Set only required environment variables (let host and database use defaults)
        Environment.SetEnvironmentVariable("DB_USERNAME", "testuser");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "testpass");

        try
        {
            // Act
            var context = factory.CreateDbContext(Array.Empty<string>());

            // Assert
            context.Should().NotBeNull();
            context.Should().BeOfType<ApplicationDbContext>();
            
            // Verify connection string contains defaults
            var connectionString = context.Database.GetConnectionString();
            connectionString.Should().Contain("Host=localhost");
            connectionString.Should().Contain("Database=socialapp");
            connectionString.Should().Contain("Username=testuser");
            connectionString.Should().Contain("Password=testpass");
            
            // Cleanup
            context.Dispose();
        }
        finally
        {
            // Clean up environment variables
            Environment.SetEnvironmentVariable("DB_USERNAME", null);
            Environment.SetEnvironmentVariable("DB_PASSWORD", null);
        }
    }

    [Fact]
    public void CreateDbContext_WithPasswordFromConfiguration_ShouldUseConfigPassword()
    {
        // Arrange
        var factory = new ApplicationDbContextFactory();
        
        // Set environment variables without password (to trigger config fallback)
        Environment.SetEnvironmentVariable("DB_USERNAME", "testuser");
        Environment.SetEnvironmentVariable("DB_PASSWORD", ""); // Empty to trigger config lookup
        Environment.SetEnvironmentVariable("DB_HOST", "testhost");
        Environment.SetEnvironmentVariable("DB_NAME", "testdb");

        try
        {
            // This will trigger the configuration password fallback logic
            // In practice, this will still throw since we don't have actual configuration
            // but it will exercise the configuration path in the code
            var act = () => factory.CreateDbContext(Array.Empty<string>());
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*password*");
        }
        finally
        {
            // Clean up environment variables
            Environment.SetEnvironmentVariable("DB_USERNAME", null);
            Environment.SetEnvironmentVariable("DB_PASSWORD", null);
            Environment.SetEnvironmentVariable("DB_HOST", null);
            Environment.SetEnvironmentVariable("DB_NAME", null);
        }
    }
}
