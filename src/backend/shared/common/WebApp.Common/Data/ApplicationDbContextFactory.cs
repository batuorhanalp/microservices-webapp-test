using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WebApp.Common.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Build connection string from environment variables and secret manager
        var connectionString = BuildConnectionString(configuration);

        // Create DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptionsAction: options =>
        {
            options.MigrationsAssembly("WebApp.Common");
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
    
    private static string BuildConnectionString(IConfiguration configuration)
    {
        // Priority: Environment Variables > Secret Manager > Configuration
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "socialapp";
        var username = Environment.GetEnvironmentVariable("DB_USERNAME") ?? 
                      throw new InvalidOperationException("DB_USERNAME environment variable is required");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? 
                      throw new InvalidOperationException("DB_PASSWORD environment variable is required");
        
        // Alternative: Get from configuration (which could be populated by secret manager)
        if (string.IsNullOrEmpty(password))
        {
            password = configuration["Database:Password"] ?? 
                      throw new InvalidOperationException("Database password not found in configuration or environment variables");
        }
        
        return $"Host={host};Database={database};Username={username};Password={password};Include Error Detail=true";
    }
}
