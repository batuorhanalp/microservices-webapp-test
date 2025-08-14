using Microsoft.EntityFrameworkCore;
using WebApp.Core.Entities;
using WebApp.Core.Interfaces;
using WebApp.Infrastructure.Data;

namespace WebApp.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the Like repository
/// </summary>
public class LikeRepository : ILikeRepository
{
    private readonly ApplicationDbContext _context;

    public LikeRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Like?> GetByIdAsync(Guid id)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Like?> GetByUserAndPostAsync(Guid userId, Guid postId)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
    }

    public async Task<IEnumerable<Like>> GetByPostAsync(Guid postId, int take, int skip)
    {
        return await _context.Likes
            .Where(l => l.PostId == postId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Like>> GetByUserAsync(Guid userId, int take, int skip)
    {
        return await _context.Likes
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task AddAsync(Like like)
    {
        if (like == null)
            throw new ArgumentNullException(nameof(like));

        await _context.Likes.AddAsync(like);
    }

    public async Task DeleteAsync(Guid id)
    {
        var like = await _context.Likes.FindAsync(id);
        if (like != null)
        {
            _context.Likes.Remove(like);
        }
    }

    public async Task DeleteByUserAndPostAsync(Guid userId, Guid postId)
    {
        var like = await _context.Likes
            .Where(l => l.UserId == userId && l.PostId == postId)
            .FirstOrDefaultAsync();
        
        if (like != null)
        {
            _context.Likes.Remove(like);
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid postId)
    {
        return await _context.Likes
            .AnyAsync(l => l.UserId == userId && l.PostId == postId);
    }

    public async Task<int> GetCountByPostAsync(Guid postId)
    {
        return await _context.Likes
            .CountAsync(l => l.PostId == postId);
    }

    public async Task<int> GetCountByUserAsync(Guid userId)
    {
        return await _context.Likes
            .CountAsync(l => l.UserId == userId);
    }

    public async Task<IEnumerable<User>> GetUsersByPostAsync(Guid postId, int limit, int offset)
    {
        return await _context.Likes
            .Where(l => l.PostId == postId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .Select(l => l.User)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
