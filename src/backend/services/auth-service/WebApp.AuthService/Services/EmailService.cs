using WebApp.Common.Interfaces;

namespace WebApp.AuthService.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink, string userName)
    {
        // TODO: Implement actual email sending (SMTP, SendGrid, etc.)
        _logger.LogInformation("Email confirmation sent to {Email} for user {UserName}. Link: {Link}", 
            email, userName, confirmationLink);
        
        // Simulate email sending delay
        await Task.Delay(100);
        
        // For now, just log the email content
        var emailContent = $@"
Hi {userName},

Welcome to WebApp! Please confirm your email address by clicking the link below:

{confirmationLink}

If you didn't create this account, you can safely ignore this email.

Best regards,
The WebApp Team";
        
        _logger.LogDebug("Email content: {Content}", emailContent);
    }

    public async Task SendPasswordResetAsync(string email, string resetLink, string userName)
    {
        _logger.LogInformation("Password reset email sent to {Email} for user {UserName}. Link: {Link}", 
            email, userName, resetLink);
        
        await Task.Delay(100);
        
        var emailContent = $@"
Hi {userName},

We received a request to reset your password. Click the link below to create a new password:

{resetLink}

This link will expire in 24 hours. If you didn't request a password reset, you can safely ignore this email.

Best regards,
The WebApp Team";
        
        _logger.LogDebug("Email content: {Content}", emailContent);
    }

    public async Task SendTwoFactorCodeAsync(string email, string code, string userName)
    {
        _logger.LogInformation("2FA code sent to {Email} for user {UserName}. Code: {Code}", 
            email, userName, code);
        
        await Task.Delay(100);
        
        var emailContent = $@"
Hi {userName},

Your two-factor authentication code is:

{code}

This code will expire in 10 minutes.

Best regards,
The WebApp Team";
        
        _logger.LogDebug("Email content: {Content}", emailContent);
    }

    public async Task SendSecurityAlertAsync(string email, string alertMessage, string userName, string? ipAddress = null, string? userAgent = null)
    {
        _logger.LogInformation("Security alert sent to {Email} for user {UserName}. Alert: {Alert}", 
            email, userName, alertMessage);
        
        await Task.Delay(100);
        
        var emailContent = $@"
Hi {userName},

Security Alert: {alertMessage}

{(ipAddress != null ? $"IP Address: {ipAddress}" : "")}
{(userAgent != null ? $"Device: {userAgent}" : "")}
Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

If this wasn't you, please change your password immediately and contact support.

Best regards,
The WebApp Security Team";
        
        _logger.LogDebug("Email content: {Content}", emailContent);
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        _logger.LogInformation("Welcome email sent to {Email} for user {UserName}", email, userName);
        
        await Task.Delay(100);
        
        var emailContent = $@"
Hi {userName},

Welcome to WebApp! Your account has been successfully created.

You can now:
- Create and share posts
- Follow other users
- Customize your profile
- And much more!

Start exploring: [App Link]

Best regards,
The WebApp Team";
        
        _logger.LogDebug("Email content: {Content}", emailContent);
    }

    public async Task SendPasswordChangedNotificationAsync(string email, string userName)
    {
        _logger.LogInformation("Password changed notification sent to {Email} for user {UserName}", email, userName);
        
        await Task.Delay(100);
        
        var emailContent = $@"
Hi {userName},

Your password has been successfully changed.

Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

If you didn't make this change, please contact support immediately.

Best regards,
The WebApp Security Team";
        
        _logger.LogDebug("Email content: {Content}", emailContent);
    }
}
