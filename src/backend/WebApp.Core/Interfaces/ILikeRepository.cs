using WebApp.Core.Entities;

namespace WebApp.Core.Interfaces;

/// <summary>
/// Repository interface for Like entity operations
/// </summary>
public interface ILikeRepository
{
    /// <summary>
    /// Gets a like by its unique identifier
    /// </summary>
    /// <param name="id">Like ID</param>
    /// <returns>Like entity or null if not found</returns>
    Task<Like?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a like by user ID and post ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="postId">Post ID</param>
    /// <returns>Like entity or null if not found</returns>
    Task<Like?> GetByUserAndPostAsync(Guid userId, Guid postId);

    /// <summary>
    /// Gets all likes for a specific post with pagination
    /// </summary>
    /// <param name="postId">Post ID to get likes for</param>
    /// <param name="take">Maximum number of likes to return</param>
    /// <param name="skip">Number of likes to skip for pagination</param>
    /// <returns>Collection of likes for the specified post</returns>
    Task<IEnumerable<Like>> GetByPostAsync(Guid postId, int take, int skip);

    /// <summary>
    /// Gets all likes by a specific user with pagination
    /// </summary>
    /// <param name="userId">User ID to get likes for</param>
    /// <param name="take">Maximum number of likes to return</param>
    /// <param name="skip">Number of likes to skip for pagination</param>
    /// <returns>Collection of likes by the specified user</returns>
    Task<IEnumerable<Like>> GetByUserAsync(Guid userId, int take, int skip);

    /// <summary>
    /// Adds a new like to the repository
    /// </summary>
    /// <param name="like">Like entity to add</param>
    Task AddAsync(Like like);

    /// <summary>
    /// Deletes a like by its ID
    /// </summary>
    /// <param name="id">Like ID to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Deletes a like by user ID and post ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="postId">Post ID</param>
    Task DeleteByUserAndPostAsync(Guid userId, Guid postId);

    /// <summary>
    /// Checks if a like exists for a specific user and post combination
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="postId">Post ID</param>
    /// <returns>True if like exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid userId, Guid postId);

    /// <summary>
    /// Gets the count of likes for a specific post
    /// </summary>
    /// <param name="postId">Post ID to count likes for</param>
    /// <returns>Number of likes for the post</returns>
    Task<int> GetCountByPostAsync(Guid postId);

    /// <summary>
    /// Gets the count of likes made by a specific user
    /// </summary>
    /// <param name="userId">User ID to count likes for</param>
    /// <returns>Number of likes made by the user</returns>
    Task<int> GetCountByUserAsync(Guid userId);

    /// <summary>
    /// Gets users who liked a specific post with pagination
    /// </summary>
    /// <param name="postId">Post ID to get users for</param>
    /// <param name="limit">Maximum number of users to return</param>
    /// <param name="offset">Number of users to skip for pagination</param>
    /// <returns>Collection of users who liked the post</returns>
    Task<IEnumerable<User>> GetUsersByPostAsync(Guid postId, int limit, int offset);

    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync();
}
