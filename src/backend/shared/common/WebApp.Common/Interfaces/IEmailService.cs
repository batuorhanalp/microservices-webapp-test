namespace WebApp.Common.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Send email confirmation message
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="confirmationLink">Email confirmation link</param>
    /// <param name="userName">User's display name</param>
    Task SendEmailConfirmationAsync(string email, string confirmationLink, string userName);
    
    /// <summary>
    /// Send password reset email
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="resetLink">Password reset link</param>
    /// <param name="userName">User's display name</param>
    Task SendPasswordResetAsync(string email, string resetLink, string userName);
    
    /// <summary>
    /// Send two-factor authentication code
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="code">2FA code</param>
    /// <param name="userName">User's display name</param>
    Task SendTwoFactorCodeAsync(string email, string code, string userName);
    
    /// <summary>
    /// Send security alert email
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="alertMessage">Security alert message</param>
    /// <param name="userName">User's display name</param>
    /// <param name="ipAddress">IP address of the activity</param>
    /// <param name="userAgent">User agent of the activity</param>
    Task SendSecurityAlertAsync(string email, string alertMessage, string userName, string? ipAddress = null, string? userAgent = null);
    
    /// <summary>
    /// Send welcome email to new users
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="userName">User's display name</param>
    Task SendWelcomeEmailAsync(string email, string userName);
    
    /// <summary>
    /// Send password changed notification
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="userName">User's display name</param>
    Task SendPasswordChangedNotificationAsync(string email, string userName);
}
