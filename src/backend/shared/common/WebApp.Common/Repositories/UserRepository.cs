using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
using WebApp.Common.Data;

namespace WebApp.Common.Repositories;

/// <summary>
/// Entity Framework implementation of IUserRepository
/// Provides data access operations for User entities
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task AddAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        await _context.Users.AddAsync(user);
    }

    public void Update(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        _context.Users.Update(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }

    public async Task<IEnumerable<User>> GetFollowersAsync(Guid userId)
    {
        return await _context.Follows
            .Where(f => f.FolloweeId == userId && f.IsAccepted)
            .Select(f => f.Follower)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetFollowingAsync(Guid userId)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == userId && f.IsAccepted)
            .Select(f => f.Followee)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> SearchByUsernameAsync(string searchTerm, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<User>();

        return await _context.Users
            .Where(u => u.Username.Contains(searchTerm))
            .Take(limit)
            .ToListAsync();
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsUsernameTakenAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return await _context.Users
            .AnyAsync(u => u.Username == username);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
