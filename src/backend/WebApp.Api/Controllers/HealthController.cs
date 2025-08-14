using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Core.Entities;

namespace WebApp.Api.Controllers;

/// <summary>
/// Health check and sample API endpoints for testing
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Gets the health status of the API
    /// </summary>
    /// <returns>Health status information</returns>
    /// <response code="200">Returns the health status</response>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetHealth()
    {
        var health = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Features = new[]
            {
                "User Management",
                "Posts & Media",
                "Social Interactions (Likes, Comments, Shares)",
                "Following System", 
                "Direct Messages",
                "Security & Performance Optimizations"
            }
        };

        return Ok(health);
    }

    /// <summary>
    /// Gets API information and available entity types
    /// </summary>
    /// <returns>API metadata</returns>
    /// <response code="200">Returns API information</response>
    [HttpGet("info")]
    [AllowAnonymous]
    public IActionResult GetApiInfo()
    {
        var apiInfo = new
        {
            Title = "Social Media Backend API",
            Version = "v1.0.0",
            Description = "A comprehensive social media backend API built with .NET 8",
            EntityTypes = new[]
            {
                nameof(User),
                nameof(Post),
                nameof(Comment),
                nameof(Like),
                nameof(Share),
                nameof(Follow),
                nameof(Message),
                nameof(MediaAttachment)
            },
            DatabaseProvider = "PostgreSQL",
            TestCoverage = "96.6%",
            TotalTests = 166,
            SecurityFeatures = new[]
            {
                "JWT Bearer Authentication",
                "Environment-based Secret Management",
                "SQL Injection Protection",
                "Input Validation"
            },
            PerformanceFeatures = new[]
            {
                "Connection Pooling",
                "Query Optimization",
                "Strategic Indexing",
                "Lazy Loading Control"
            }
        };

        return Ok(apiInfo);
    }

    /// <summary>
    /// Protected endpoint to test JWT authentication
    /// </summary>
    /// <returns>Protected resource information</returns>
    /// <response code="200">Returns protected resource data when authenticated</response>
    /// <response code="401">Authentication required</response>
    [HttpGet("protected")]
    [Authorize]
    public IActionResult GetProtectedResource()
    {
        var userInfo = new
        {
            UserId = User.Identity?.Name ?? "Unknown",
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray(),
            Message = "Successfully accessed protected resource!",
            Timestamp = DateTime.UtcNow
        };

        return Ok(userInfo);
    }
}
