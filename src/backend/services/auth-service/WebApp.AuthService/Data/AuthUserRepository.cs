using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.AuthService.Data;

/// <summary>
/// Auth-specific extended user repository with additional methods
/// </summary>
public class AuthUserRepository : UserRepository
{
    private readonly AuthDbContext _context;

    public AuthUserRepository(AuthDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Get user by email or username (for login)
    /// </summary>
    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailOrUsername.ToLower() 
                                   || u.Username.ToLower() == emailOrUsername.ToLower());
    }

    /// <summary>
    /// Get user by email confirmation token
    /// </summary>
    public async Task<User?> GetByEmailConfirmationTokenAsync(string token)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
    }

    /// <summary>
    /// Create user with automatic save
    /// </summary>
    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Update user with automatic save
    /// </summary>
    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Delete user entity with automatic save
    /// </summary>
    public async Task DeleteUserAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Check if email exists
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Check if username exists
    /// </summary>
    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }
}
