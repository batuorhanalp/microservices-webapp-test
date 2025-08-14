using WebApp.Core.Entities;

namespace WebApp.Core.Interfaces;

/// <summary>
/// Repository interface for Post entity operations
/// Defines the contract for post data access
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Gets a post by its unique identifier
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <returns>Post entity or null if not found</returns>
    Task<Post?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets posts by a specific author with pagination
    /// </summary>
    /// <param name="authorId">Author's user ID</param>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <param name="offset">Number of posts to skip</param>
    /// <returns>List of posts by the author</returns>
    Task<IEnumerable<Post>> GetByAuthorAsync(Guid authorId, int limit = 20, int offset = 0);

    /// <summary>
    /// Gets the personalized feed for a user (posts from followed users)
    /// </summary>
    /// <param name="userId">User ID to get feed for</param>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <param name="offset">Number of posts to skip</param>
    /// <returns>List of posts from followed users</returns>
    Task<IEnumerable<Post>> GetFeedAsync(Guid userId, int limit = 20, int offset = 0);

    /// <summary>
    /// Gets public timeline posts (all public posts) with pagination
    /// </summary>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <param name="offset">Number of posts to skip</param>
    /// <returns>List of public posts ordered by creation date</returns>
    Task<IEnumerable<Post>> GetPublicTimelineAsync(int limit = 20, int offset = 0);

    /// <summary>
    /// Gets replies to a specific post
    /// </summary>
    /// <param name="postId">Parent post ID</param>
    /// <param name="limit">Maximum number of replies to return</param>
    /// <param name="offset">Number of replies to skip</param>
    /// <returns>List of reply posts</returns>
    Task<IEnumerable<Post>> GetRepliesAsync(Guid postId, int limit = 20, int offset = 0);

    /// <summary>
    /// Gets a post with its media attachments included
    /// </summary>
    /// <param name="postId">Post ID</param>
    /// <returns>Post with media attachments or null if not found</returns>
    Task<Post?> GetWithMediaAsync(Guid postId);

    /// <summary>
    /// Searches posts by content
    /// </summary>
    /// <param name="searchTerm">Term to search for in post content</param>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <param name="offset">Number of posts to skip</param>
    /// <returns>List of posts matching the search term</returns>
    Task<IEnumerable<Post>> SearchAsync(string searchTerm, int limit = 20, int offset = 0);

    /// <summary>
    /// Gets trending posts (posts with high engagement)
    /// </summary>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <param name="offset">Number of posts to skip</param>
    /// <returns>List of trending posts</returns>
    Task<IEnumerable<Post>> GetTrendingAsync(int limit = 20, int offset = 0);

    /// <summary>
    /// Adds a new post to the repository
    /// </summary>
    /// <param name="post">Post entity to add</param>
    Task AddAsync(Post post);

    /// <summary>
    /// Updates an existing post
    /// </summary>
    /// <param name="post">Post entity to update</param>
    void Update(Post post);

    /// <summary>
    /// Deletes a post by its ID
    /// </summary>
    /// <param name="id">Post ID to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets the number of likes for a specific post
    /// </summary>
    /// <param name="postId">Post ID</param>
    /// <returns>Number of likes</returns>
    Task<int> GetLikeCountAsync(Guid postId);

    /// <summary>
    /// Gets the number of comments for a specific post
    /// </summary>
    /// <param name="postId">Post ID</param>
    /// <returns>Number of comments</returns>
    Task<int> GetCommentCountAsync(Guid postId);

    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync();
}
