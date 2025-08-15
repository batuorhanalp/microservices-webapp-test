var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with microservices aggregation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WebApp API Gateway", Version = "v1" });
    
    // Configure JWT authentication for Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add HTTP client for proxying requests to microservices
builder.Services.AddHttpClient();

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        
        // Add endpoints for each microservice
        c.SwaggerEndpoint("http://localhost:7001/swagger/v1/swagger.json", "Auth Service");
        c.SwaggerEndpoint("http://localhost:7002/swagger/v1/swagger.json", "User Service");
        c.SwaggerEndpoint("http://localhost:7003/swagger/v1/swagger.json", "Post Service");
        c.SwaggerEndpoint("http://localhost:7004/swagger/v1/swagger.json", "Like Service");
        c.SwaggerEndpoint("http://localhost:7005/swagger/v1/swagger.json", "Comment Service");
        c.SwaggerEndpoint("http://localhost:7006/swagger/v1/swagger.json", "Notification Service");
        c.SwaggerEndpoint("http://localhost:7007/swagger/v1/swagger.json", "Media Upload Service");
        c.SwaggerEndpoint("http://localhost:7008/swagger/v1/swagger.json", "Media Processing Service");
        
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "WebApp Microservices API Documentation";
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow, Services = "API Gateway" });

// Add service discovery endpoint
app.MapGet("/services", () => new
{
    Services = new[]
    {
        new { Name = "Auth Service", Url = "http://localhost:7001", SwaggerUrl = "http://localhost:7001/swagger" },
        new { Name = "User Service", Url = "http://localhost:7002", SwaggerUrl = "http://localhost:7002/swagger" },
        new { Name = "Post Service", Url = "http://localhost:7003", SwaggerUrl = "http://localhost:7003/swagger" },
        new { Name = "Like Service", Url = "http://localhost:7004", SwaggerUrl = "http://localhost:7004/swagger" },
        new { Name = "Comment Service", Url = "http://localhost:7005", SwaggerUrl = "http://localhost:7005/swagger" },
        new { Name = "Notification Service", Url = "http://localhost:7006", SwaggerUrl = "http://localhost:7006/swagger" },
        new { Name = "Media Upload Service", Url = "http://localhost:7007", SwaggerUrl = "http://localhost:7007/swagger" },
        new { Name = "Media Processing Service", Url = "http://localhost:7008", SwaggerUrl = "http://localhost:7008/swagger" }
    }
});

app.Run();
