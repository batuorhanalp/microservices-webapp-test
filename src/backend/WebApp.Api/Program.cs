using Microsoft.EntityFrameworkCore;
using WebApp.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add secret management configuration
if (builder.Environment.IsProduction())
{
    // In production, use Key Vault or equivalent secret manager
    // builder.Configuration.AddAzureKeyVault(...);
    // builder.Configuration.AddAWSSecretsManager(...);
    // builder.Configuration.AddGoogleSecretManager(...);
}

// Add services to the container.
builder.Services.AddControllers();

// Add Entity Framework with secure connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = BuildSecureConnectionString(builder.Configuration);
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    });
    
    // Performance optimizations
    if (!builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging(false);
        options.EnableDetailedErrors(false);
    }
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Social Media API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Social Media API v1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string BuildSecureConnectionString(IConfiguration configuration)
{
    // Priority: Environment Variables > Secret Manager > Configuration
    var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
    var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "socialapp";
    var username = Environment.GetEnvironmentVariable("DB_USERNAME") ?? 
                  throw new InvalidOperationException("DB_USERNAME environment variable is required");
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? 
                  throw new InvalidOperationException("DB_PASSWORD environment variable is required");
    
    // Alternative: Get from configuration (populated by secret manager)
    if (string.IsNullOrEmpty(password))
    {
        password = configuration["Database:Password"] ?? 
                  throw new InvalidOperationException("Database password not found in configuration or environment variables");
    }
    
    return $"Host={host};Database={database};Username={username};Password={password};" +
           "Pooling=true;MinPoolSize=5;MaxPoolSize=100;ConnectionIdleLifetime=300;" +
           "Include Error Detail=true;Trust Server Certificate=false";
}
