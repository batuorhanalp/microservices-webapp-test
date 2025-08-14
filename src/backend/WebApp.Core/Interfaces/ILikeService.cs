using WebApp.Core.Entities;

namespace WebApp.Core.Interfaces;

/// <summary>
/// Interface for like-related business operations
/// Handles liking/unliking posts and retrieving like information
/// </summary>
public interface ILikeService
{
    /// <summary>
    /// Likes a post by a user
    /// </summary>
    /// <param name="userId">ID of the user liking the post</param>
    /// <param name="postId">ID of the post to like</param>
    /// <returns>The created like entity</returns>
    /// <exception cref="ArgumentException">Thrown when user or post doesn't exist, or when like already exists</exception>
    Task<Like> LikePostAsync(Guid userId, Guid postId);

    /// <summary>
    /// Unlikes a post by a user
    /// </summary>
    /// <param name="userId">ID of the user unliking the post</param>
    /// <param name="postId">ID of the post to unlike</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentException">Thrown when user or post doesn't exist, or when like doesn't exist</exception>
    Task UnlikePostAsync(Guid userId, Guid postId);

    /// <summary>
    /// Checks if a user has liked a specific post
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="postId">ID of the post</param>
    /// <returns>True if the user has liked the post, false otherwise</returns>
    Task<bool> HasUserLikedPostAsync(Guid userId, Guid postId);

    /// <summary>
    /// Gets all likes for a specific post
    /// </summary>
    /// <param name="postId">ID of the post</param>
    /// <param name="limit">Maximum number of likes to return (default: 50)</param>
    /// <param name="offset">Number of likes to skip for pagination (default: 0)</param>
    /// <returns>Collection of likes for the post</returns>
    Task<IEnumerable<Like>> GetPostLikesAsync(Guid postId, int limit = 50, int offset = 0);

    /// <summary>
    /// Gets all likes made by a specific user
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="limit">Maximum number of likes to return (default: 50)</param>
    /// <param name="offset">Number of likes to skip for pagination (default: 0)</param>
    /// <returns>Collection of likes made by the user</returns>
    Task<IEnumerable<Like>> GetUserLikesAsync(Guid userId, int limit = 50, int offset = 0);

    /// <summary>
    /// Gets the total number of likes for a specific post
    /// </summary>
    /// <param name="postId">ID of the post</param>
    /// <returns>Number of likes for the post</returns>
    Task<int> GetPostLikeCountAsync(Guid postId);

    /// <summary>
    /// Gets the total number of likes made by a specific user
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <returns>Number of likes made by the user</returns>
    Task<int> GetUserLikeCountAsync(Guid userId);

    /// <summary>
    /// Gets users who liked a specific post
    /// </summary>
    /// <param name="postId">ID of the post</param>
    /// <param name="limit">Maximum number of users to return (default: 50)</param>
    /// <param name="offset">Number of users to skip for pagination (default: 0)</param>
    /// <returns>Collection of users who liked the post</returns>
    Task<IEnumerable<User>> GetUsersWhoLikedPostAsync(Guid postId, int limit = 50, int offset = 0);
}
