using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace WebApp.Gateway.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    // Actual port where Auth service is running
    private const string AUTH_SERVICE_URL = "http://localhost:7001";

    public AuthController(HttpClient httpClient, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] object loginData)
    {
        try
        {
            _logger.LogInformation("Proxying login request to Auth service");
            
            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{AUTH_SERVICE_URL}/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return StatusCode((int)response.StatusCode, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying login request");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] object registerData)
    {
        try
        {
            _logger.LogInformation("Proxying register request to Auth service");
            
            var json = JsonSerializer.Serialize(registerData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{AUTH_SERVICE_URL}/api/auth/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return StatusCode((int)response.StatusCode, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying register request");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] object refreshData)
    {
        try
        {
            _logger.LogInformation("Proxying refresh request to Auth service");
            
            var json = JsonSerializer.Serialize(refreshData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{AUTH_SERVICE_URL}/api/auth/refresh", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return StatusCode((int)response.StatusCode, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying refresh request");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            _logger.LogInformation("Proxying logout request to Auth service");
            
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authHeader);
            }
            
            var response = await _httpClient.PostAsync($"{AUTH_SERVICE_URL}/api/auth/logout", null);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return StatusCode((int)response.StatusCode, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying logout request");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            _logger.LogInformation("Proxying profile request to Auth service");
            
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authHeader);
            }
            
            var response = await _httpClient.GetAsync($"{AUTH_SERVICE_URL}/api/auth/profile");
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return StatusCode((int)response.StatusCode, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying profile request");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] object forgotPasswordData)
    {
        try
        {
            _logger.LogInformation("Proxying forgot password request to Auth service");
            
            var json = JsonSerializer.Serialize(forgotPasswordData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{AUTH_SERVICE_URL}/api/auth/forgot-password", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return StatusCode((int)response.StatusCode, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying forgot password request");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] object resetPasswordData)
    {
        try
        {
            _logger.LogInformation("Proxying reset password request to Auth service");
            
            var json = JsonSerializer.Serialize(resetPasswordData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{AUTH_SERVICE_URL}/api/auth/reset-password", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return StatusCode((int)response.StatusCode, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying reset password request");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] object verifyEmailData)
    {
        try
        {
            _logger.LogInformation("Proxying verify email request to Auth service");
            
            var json = JsonSerializer.Serialize(verifyEmailData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{AUTH_SERVICE_URL}/api/auth/verify-email", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return StatusCode((int)response.StatusCode, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying verify email request");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
