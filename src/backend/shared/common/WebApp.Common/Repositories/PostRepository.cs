using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
using WebApp.Common.Data;

namespace WebApp.Common.Repositories;

/// <summary>
/// Entity Framework implementation of IPostRepository
/// Provides data access operations for Post entities
/// </summary>
public class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _context;

    public PostRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Post>> GetByAuthorAsync(Guid authorId, int limit = 20, int offset = 0)
    {
        return await _context.Posts
            .Where(p => p.AuthorId == authorId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetFeedAsync(Guid userId, int limit = 20, int offset = 0)
    {
        // Get posts from users that the current user follows
        return await _context.Posts
            .Where(p => _context.Follows
                .Where(f => f.FollowerId == userId && f.IsAccepted)
                .Select(f => f.FolloweeId)
                .Contains(p.AuthorId))
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPublicTimelineAsync(int limit = 20, int offset = 0)
    {
        return await _context.Posts
            .Where(p => p.Visibility == PostVisibility.Public)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetRepliesAsync(Guid postId, int limit = 20, int offset = 0)
    {
        return await _context.Posts
            .Where(p => p.ParentPostId == postId)
            .OrderBy(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Post?> GetWithMediaAsync(Guid postId)
    {
        return await _context.Posts
            .Include(p => p.MediaAttachments)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<IEnumerable<Post>> GetPostsWithMediaAsync(int limit = 20, int offset = 0)
    {
        return await _context.Posts
            .Include(p => p.MediaAttachments)
            .Where(p => p.MediaAttachments.Any())
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> SearchAsync(string searchTerm, int limit = 20, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<Post>();

        return await _context.Posts
            .Where(p => p.Content.Contains(searchTerm) && p.Visibility == PostVisibility.Public)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetTrendingAsync(int limit = 20, int offset = 0)
    {
        // Simplified trending algorithm - posts with most likes in recent period
        var recentCutoff = DateTime.UtcNow.AddDays(-7); // Last 7 days
        
        return await _context.Posts
            .Where(p => p.Visibility == PostVisibility.Public && p.CreatedAt >= recentCutoff)
            .Select(p => new { 
                Post = p, 
                LikeCount = _context.Likes.Count(l => l.PostId == p.Id) 
            })
            .OrderByDescending(x => x.LikeCount)
            .ThenByDescending(x => x.Post.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .Select(x => x.Post)
            .ToListAsync();
    }

    public async Task AddAsync(Post post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post));

        await _context.Posts.AddAsync(post);
    }

    public void Update(Post post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post));

        _context.Posts.Update(post);
    }

    public async Task DeleteAsync(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            _context.Posts.Remove(post);
        }
    }

    public async Task<int> GetLikeCountAsync(Guid postId)
    {
        return await _context.Likes
            .CountAsync(l => l.PostId == postId);
    }

    public async Task<int> GetCommentCountAsync(Guid postId)
    {
        return await _context.Comments
            .CountAsync(c => c.PostId == postId);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
