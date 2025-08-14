using System.Security.Cryptography;
using System.Text;
using WebApp.Common.Interfaces;

namespace WebApp.Common.Services;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 32; // 256 bits
    private const int KeySize = 64; // 512 bits
    private const int Iterations = 350000;
    
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));
            
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }
    
    public string HashPassword(string password, string salt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));
            
        if (string.IsNullOrWhiteSpace(salt))
            throw new ArgumentException("Salt cannot be null or whitespace.", nameof(salt));
            
        using var algorithm = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password),
            Convert.FromBase64String(salt),
            Iterations,
            HashAlgorithmName.SHA256);
            
        return Convert.ToBase64String(algorithm.GetBytes(KeySize));
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
            
        if (string.IsNullOrWhiteSpace(hash))
            return false;
            
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
    
    public bool VerifyPassword(string password, string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(salt))
            return false;
            
        try
        {
            using var algorithm = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(password),
                Convert.FromBase64String(salt),
                Iterations,
                HashAlgorithmName.SHA256);
                
            var keyToCheck = algorithm.GetBytes(KeySize);
            var hashToCheck = Convert.FromBase64String(hash);
            
            return CryptographicOperations.FixedTimeEquals(keyToCheck, hashToCheck);
        }
        catch
        {
            return false;
        }
    }
    
    public string GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[SaltSize];
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
    
    public string GenerateRandomPassword(int length = 12, bool includeSymbols = true)
    {
        if (length < 4)
            throw new ArgumentException("Password length must be at least 4 characters.", nameof(length));
            
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        
        var characterPool = lowercase + uppercase + digits;
        if (includeSymbols)
            characterPool += symbols;
            
        var password = new StringBuilder();
        using var rng = RandomNumberGenerator.Create();
        
        // Ensure at least one character from each required type
        password.Append(GetRandomCharacter(lowercase, rng));
        password.Append(GetRandomCharacter(uppercase, rng));
        password.Append(GetRandomCharacter(digits, rng));
        
        if (includeSymbols && length > 3)
        {
            password.Append(GetRandomCharacter(symbols, rng));
        }
        
        // Fill remaining length with random characters
        var remaining = length - password.Length;
        for (int i = 0; i < remaining; i++)
        {
            password.Append(GetRandomCharacter(characterPool, rng));
        }
        
        // Shuffle the password
        return ShuffleString(password.ToString(), rng);
    }
    
    private static char GetRandomCharacter(string characters, RandomNumberGenerator rng)
    {
        var data = new byte[4];
        rng.GetBytes(data);
        var randomValue = BitConverter.ToUInt32(data, 0);
        return characters[(int)(randomValue % characters.Length)];
    }
    
    private static string ShuffleString(string input, RandomNumberGenerator rng)
    {
        var array = input.ToCharArray();
        var data = new byte[4];
        
        for (int i = array.Length - 1; i > 0; i--)
        {
            rng.GetBytes(data);
            var randomValue = BitConverter.ToUInt32(data, 0);
            var j = (int)(randomValue % (i + 1));
            
            (array[i], array[j]) = (array[j], array[i]);
        }
        
        return new string(array);
    }
}
