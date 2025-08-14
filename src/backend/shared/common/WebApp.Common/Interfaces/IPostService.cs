using WebApp.Common.Entities;

namespace WebApp.Common.Interfaces;

/// <summary>
/// Service interface for managing post-related business logic operations.
/// Implements comprehensive post management including creation, updates, visibility control,
/// and media attachment handling with proper validation and business rules.
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Creates a new text post with validation and business logic.
    /// </summary>
    /// <param name="authorId">ID of the user creating the post</param>
    /// <param name="content">Post content text</param>
    /// <param name="visibility">Post visibility level</param>
    /// <returns>The created post</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<Post> CreateTextPostAsync(Guid authorId, string content, PostVisibility visibility = PostVisibility.Public);

    /// <summary>
    /// Creates a new media post with attachment validation.
    /// </summary>
    /// <param name="authorId">ID of the user creating the post</param>
    /// <param name="content">Optional post content text</param>
    /// <param name="mediaAttachments">List of media attachments</param>
    /// <param name="visibility">Post visibility level</param>
    /// <returns>The created post</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<Post> CreateMediaPostAsync(Guid authorId, string? content, IEnumerable<MediaAttachment> mediaAttachments, PostVisibility visibility = PostVisibility.Public);

    /// <summary>
    /// Creates a reply post to an existing post.
    /// </summary>
    /// <param name="authorId">ID of the user creating the reply</param>
    /// <param name="parentPostId">ID of the post being replied to</param>
    /// <param name="content">Reply content</param>
    /// <param name="visibility">Reply visibility level</param>
    /// <returns>The created reply post</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<Post> CreateReplyAsync(Guid authorId, Guid parentPostId, string content, PostVisibility visibility = PostVisibility.Public);

    /// <summary>
    /// Updates an existing post's content with validation and authorization.
    /// </summary>
    /// <param name="postId">ID of the post to update</param>
    /// <param name="authorId">ID of the user requesting the update</param>
    /// <param name="newContent">New content for the post</param>
    /// <returns>The updated post</returns>
    /// <exception cref="ArgumentException">Thrown when validation or authorization fails</exception>
    Task<Post> UpdatePostContentAsync(Guid postId, Guid authorId, string newContent);

    /// <summary>
    /// Updates a post's visibility with authorization check.
    /// </summary>
    /// <param name="postId">ID of the post to update</param>
    /// <param name="authorId">ID of the user requesting the update</param>
    /// <param name="newVisibility">New visibility level</param>
    /// <returns>The updated post</returns>
    /// <exception cref="ArgumentException">Thrown when validation or authorization fails</exception>
    Task<Post> UpdatePostVisibilityAsync(Guid postId, Guid authorId, PostVisibility newVisibility);

    /// <summary>
    /// Deletes a post with proper authorization and cascade handling.
    /// </summary>
    /// <param name="postId">ID of the post to delete</param>
    /// <param name="authorId">ID of the user requesting the deletion</param>
    /// <returns>True if deletion was successful</returns>
    /// <exception cref="ArgumentException">Thrown when validation or authorization fails</exception>
    Task<bool> DeletePostAsync(Guid postId, Guid authorId);

    /// <summary>
    /// Retrieves a post by ID with proper visibility and permission checks.
    /// </summary>
    /// <param name="postId">ID of the post to retrieve</param>
    /// <param name="viewerId">ID of the user viewing the post (null for anonymous)</param>
    /// <returns>The post if found and accessible, null otherwise</returns>
    Task<Post?> GetPostByIdAsync(Guid postId, Guid? viewerId = null);

    /// <summary>
    /// Retrieves posts by author with pagination and visibility filtering.
    /// </summary>
    /// <param name="authorId">ID of the author</param>
    /// <param name="viewerId">ID of the user viewing the posts (null for anonymous)</param>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <param name="offset">Number of posts to skip for pagination</param>
    /// <returns>List of accessible posts by the author</returns>
    Task<IEnumerable<Post>> GetPostsByAuthorAsync(Guid authorId, Guid? viewerId = null, int limit = 20, int offset = 0);

    /// <summary>
    /// Retrieves posts for a user's feed with proper visibility filtering.
    /// </summary>
    /// <param name="userId">ID of the user requesting their feed</param>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <param name="offset">Number of posts to skip for pagination</param>
    /// <returns>List of posts for the user's feed</returns>
    Task<IEnumerable<Post>> GetUserFeedAsync(Guid userId, int limit = 20, int offset = 0);

    /// <summary>
    /// Retrieves public posts for the global timeline.
    /// </summary>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <param name="offset">Number of posts to skip for pagination</param>
    /// <returns>List of public posts</returns>
    Task<IEnumerable<Post>> GetPublicTimelineAsync(int limit = 20, int offset = 0);

    /// <summary>
    /// Searches posts with content filtering and visibility checks.
    /// </summary>
    /// <param name="searchTerm">Search term to match in post content</param>
    /// <param name="viewerId">ID of the user performing the search (null for anonymous)</param>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <returns>List of matching accessible posts</returns>
    Task<IEnumerable<Post>> SearchPostsAsync(string searchTerm, Guid? viewerId = null, int limit = 20);

    /// <summary>
    /// Checks if a user can view a specific post based on visibility rules.
    /// </summary>
    /// <param name="postId">ID of the post to check</param>
    /// <param name="viewerId">ID of the user requesting to view (null for anonymous)</param>
    /// <returns>True if the post can be viewed, false otherwise</returns>
    Task<bool> CanUserViewPostAsync(Guid postId, Guid? viewerId);

    /// <summary>
    /// Gets posts with media attachments for media-specific feeds.
    /// </summary>
    /// <param name="viewerId">ID of the user requesting media posts</param>
    /// <param name="limit">Maximum number of posts to return</param>
    /// <param name="offset">Number of posts to skip for pagination</param>
    /// <returns>List of posts containing media attachments</returns>
    Task<IEnumerable<Post>> GetMediaPostsAsync(Guid? viewerId = null, int limit = 20, int offset = 0);

    /// <summary>
    /// Gets reply posts for a specific post.
    /// </summary>
    /// <param name="parentPostId">ID of the parent post</param>
    /// <param name="viewerId">ID of the user requesting replies (null for anonymous)</param>
    /// <param name="limit">Maximum number of replies to return</param>
    /// <param name="offset">Number of replies to skip for pagination</param>
    /// <returns>List of accessible reply posts</returns>
    Task<IEnumerable<Post>> GetPostRepliesAsync(Guid parentPostId, Guid? viewerId = null, int limit = 20, int offset = 0);
}
