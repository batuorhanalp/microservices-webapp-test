using WebApp.Core.Entities;

namespace WebApp.Core.Interfaces;

/// <summary>
/// Repository interface for Comment entity operations
/// </summary>
public interface ICommentRepository
{
    /// <summary>
    /// Gets a comment by its unique identifier
    /// </summary>
    /// <param name="id">Comment ID</param>
    /// <returns>Comment entity or null if not found</returns>
    Task<Comment?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a new comment to the repository
    /// </summary>
    /// <param name="comment">Comment entity to add</param>
    Task AddAsync(Comment comment);

    /// <summary>
    /// Updates an existing comment
    /// </summary>
    /// <param name="comment">Comment entity to update</param>
    void Update(Comment comment);

    /// <summary>
    /// Deletes a comment by its ID
    /// </summary>
    /// <param name="id">Comment ID to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync();
    /// <summary>
    /// Retrieves comments for a specific post with pagination and ordering
    /// </summary>
    /// <param name="postId">The post ID to get comments for</param>
    /// <param name="take">Maximum number of comments to return</param>
    /// <param name="skip">Number of comments to skip for pagination</param>
    /// <returns>Collection of comments ordered by creation date (ascending)</returns>
    Task<IEnumerable<Comment>> GetByPostAsync(Guid postId, int take, int skip);

    /// <summary>
    /// Retrieves comments by a specific author with pagination
    /// </summary>
    /// <param name="authorId">The author ID to get comments for</param>
    /// <param name="take">Maximum number of comments to return</param>
    /// <param name="skip">Number of comments to skip for pagination</param>
    /// <returns>Collection of comments by the specified author</returns>
    Task<IEnumerable<Comment>> GetByAuthorAsync(Guid authorId, int take, int skip);

}
