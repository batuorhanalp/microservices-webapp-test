namespace WebApp.Common.Interfaces;

public interface IPasswordService
{
    /// <summary>
    /// Hash a plain text password
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Hash a plain text password with custom salt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="salt">Custom salt</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password, string salt);
    
    /// <summary>
    /// Verify a password against its hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">Stored password hash</param>
    /// <returns>True if password matches</returns>
    bool VerifyPassword(string password, string hash);
    
    /// <summary>
    /// Verify a password against its hash with custom salt
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">Stored password hash</param>
    /// <param name="salt">Custom salt</param>
    /// <returns>True if password matches</returns>
    bool VerifyPassword(string password, string hash, string salt);
    
    /// <summary>
    /// Generate a random salt
    /// </summary>
    /// <returns>Random salt string</returns>
    string GenerateSalt();
    
    /// <summary>
    /// Generate a secure random password
    /// </summary>
    /// <param name="length">Password length</param>
    /// <param name="includeSymbols">Include special symbols</param>
    /// <returns>Random password</returns>
    string GenerateRandomPassword(int length = 12, bool includeSymbols = true);
}
