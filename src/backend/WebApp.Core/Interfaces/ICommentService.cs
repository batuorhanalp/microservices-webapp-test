using WebApp.Core.Entities;

namespace WebApp.Core.Interfaces;

/// <summary>
/// Interface for comment-related business operations
/// Handles comment creation, updates, deletion, and retrieval with proper threading support
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Creates a new comment on a post
    /// </summary>
    /// <param name="authorId">ID of the user creating the comment</param>
    /// <param name="postId">ID of the post being commented on</param>
    /// <param name="content">Content of the comment</param>
    /// <returns>The created comment entity</returns>
    /// <exception cref="ArgumentException">Thrown when user or post doesn't exist, or content is invalid</exception>
    Task<Comment> CreateCommentAsync(Guid authorId, Guid postId, string content);

    /// <summary>
    /// Creates a reply to an existing comment
    /// </summary>
    /// <param name="authorId">ID of the user creating the reply</param>
    /// <param name="parentCommentId">ID of the comment being replied to</param>
    /// <param name="content">Content of the reply</param>
    /// <returns>The created reply comment entity</returns>
    /// <exception cref="ArgumentException">Thrown when user or parent comment doesn't exist, or content is invalid</exception>
    Task<Comment> CreateReplyAsync(Guid authorId, Guid parentCommentId, string content);

    /// <summary>
    /// Updates the content of an existing comment
    /// </summary>
    /// <param name="commentId">ID of the comment to update</param>
    /// <param name="authorId">ID of the user updating the comment (must be the original author)</param>
    /// <param name="newContent">New content for the comment</param>
    /// <returns>The updated comment entity</returns>
    /// <exception cref="ArgumentException">Thrown when comment doesn't exist, user is not authorized, or content is invalid</exception>
    Task<Comment> UpdateCommentAsync(Guid commentId, Guid authorId, string newContent);

    /// <summary>
    /// Deletes a comment (soft delete - marks as deleted but preserves for threading)
    /// </summary>
    /// <param name="commentId">ID of the comment to delete</param>
    /// <param name="authorId">ID of the user deleting the comment (must be the original author)</param>
    /// <returns>True if deletion was successful</returns>
    /// <exception cref="ArgumentException">Thrown when comment doesn't exist or user is not authorized</exception>
    Task<bool> DeleteCommentAsync(Guid commentId, Guid authorId);

    /// <summary>
    /// Gets a comment by its ID
    /// </summary>
    /// <param name="commentId">ID of the comment to retrieve</param>
    /// <param name="viewerId">Optional ID of the user viewing the comment (for authorization)</param>
    /// <returns>The comment entity or null if not found/not authorized</returns>
    Task<Comment?> GetCommentByIdAsync(Guid commentId, Guid? viewerId = null);

    /// <summary>
    /// Gets all comments for a specific post with pagination
    /// </summary>
    /// <param name="postId">ID of the post to get comments for</param>
    /// <param name="viewerId">Optional ID of the user viewing comments (for authorization)</param>
    /// <param name="limit">Maximum number of comments to return (default: 50)</param>
    /// <param name="offset">Number of comments to skip for pagination (default: 0)</param>
    /// <returns>Collection of comments for the post</returns>
    Task<IEnumerable<Comment>> GetPostCommentsAsync(Guid postId, Guid? viewerId = null, int limit = 50, int offset = 0);

    /// <summary>
    /// Gets all comments made by a specific user with pagination
    /// </summary>
    /// <param name="userId">ID of the user to get comments for</param>
    /// <param name="viewerId">Optional ID of the user viewing comments (for authorization)</param>
    /// <param name="limit">Maximum number of comments to return (default: 50)</param>
    /// <param name="offset">Number of comments to skip for pagination (default: 0)</param>
    /// <returns>Collection of comments made by the user</returns>
    Task<IEnumerable<Comment>> GetUserCommentsAsync(Guid userId, Guid? viewerId = null, int limit = 50, int offset = 0);

    /// <summary>
    /// Gets replies to a specific comment with pagination
    /// </summary>
    /// <param name="parentCommentId">ID of the parent comment to get replies for</param>
    /// <param name="viewerId">Optional ID of the user viewing replies (for authorization)</param>
    /// <param name="limit">Maximum number of replies to return (default: 20)</param>
    /// <param name="offset">Number of replies to skip for pagination (default: 0)</param>
    /// <returns>Collection of reply comments</returns>
    Task<IEnumerable<Comment>> GetCommentRepliesAsync(Guid parentCommentId, Guid? viewerId = null, int limit = 20, int offset = 0);

    /// <summary>
    /// Gets the total number of comments for a specific post
    /// </summary>
    /// <param name="postId">ID of the post to count comments for</param>
    /// <returns>Number of comments for the post</returns>
    Task<int> GetPostCommentCountAsync(Guid postId);

    /// <summary>
    /// Gets the total number of comments made by a specific user
    /// </summary>
    /// <param name="userId">ID of the user to count comments for</param>
    /// <returns>Number of comments made by the user</returns>
    Task<int> GetUserCommentCountAsync(Guid userId);

    /// <summary>
    /// Gets the total number of replies to a specific comment
    /// </summary>
    /// <param name="parentCommentId">ID of the parent comment to count replies for</param>
    /// <returns>Number of replies to the comment</returns>
    Task<int> GetCommentReplyCountAsync(Guid parentCommentId);
}
