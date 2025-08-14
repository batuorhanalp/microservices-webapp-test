using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.AuthService.Data;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
    }

    public void Update(User user)
    {
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
        // For auth service, we don't need follower functionality, return empty list
        return await Task.FromResult(new List<User>());
    }

    public async Task<IEnumerable<User>> GetFollowingAsync(Guid userId)
    {
        // For auth service, we don't need following functionality, return empty list
        return await Task.FromResult(new List<User>());
    }

    public async Task<IEnumerable<User>> SearchByUsernameAsync(string searchTerm, int limit = 20)
    {
        return await _context.Users
            .Where(u => u.Username.ToLower().Contains(searchTerm.ToLower()))
            .Take(limit)
            .ToListAsync();
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> IsUsernameTakenAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
