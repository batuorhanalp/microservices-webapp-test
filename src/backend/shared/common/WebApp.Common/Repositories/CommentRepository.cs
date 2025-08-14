using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
using WebApp.Common.Data;

namespace WebApp.Common.Repositories;

/// <summary>
/// EF Core implementation of the Comment repository
/// </summary>
public class CommentRepository : ICommentRepository
{
    private readonly ApplicationDbContext _context;

    public CommentRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Comment comment)
    {
        if (comment == null)
            throw new ArgumentNullException(nameof(comment));

        await _context.Comments.AddAsync(comment);
    }

    public void Update(Comment comment)
    {
        if (comment == null)
            throw new ArgumentNullException(nameof(comment));

        _context.Comments.Update(comment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Comment>> GetByPostAsync(Guid postId, int take, int skip)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetByAuthorAsync(Guid authorId, int take, int skip)
    {
        return await _context.Comments
            .Where(c => c.UserId == authorId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}
