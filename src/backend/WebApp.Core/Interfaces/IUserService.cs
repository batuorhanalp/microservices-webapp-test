using WebApp.Core.Entities;

namespace WebApp.Core.Interfaces;

/// <summary>
/// Service interface for User business logic operations
/// Handles complex business rules, validation, and orchestration
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user with validation and business rules
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="username">Unique username</param>
    /// <param name="displayName">User's display name</param>
    /// <returns>Created user entity</returns>
    /// <exception cref="ArgumentException">Thrown when email/username is already taken or invalid</exception>
    Task<User> CreateUserAsync(string email, string username, string displayName);

    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    /// <param name="userId">User ID to retrieve</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Updates user profile information with validation
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="displayName">New display name</param>
    /// <param name="bio">New bio information (optional)</param>
    /// <returns>Updated user entity</returns>
    /// <exception cref="ArgumentException">Thrown when user is not found</exception>
    Task<User> UpdateUserProfileAsync(Guid userId, string displayName, string? bio = null);

    /// <summary>
    /// Creates a follow relationship between users with business rules validation
    /// </summary>
    /// <param name="followerId">ID of the user who wants to follow</param>
    /// <param name="followeeId">ID of the user to be followed</param>
    /// <exception cref="ArgumentException">Thrown when users don't exist or business rules are violated</exception>
    Task FollowUserAsync(Guid followerId, Guid followeeId);

    /// <summary>
    /// Removes a follow relationship between users
    /// </summary>
    /// <param name="followerId">ID of the user who wants to unfollow</param>
    /// <param name="followeeId">ID of the user to be unfollowed</param>
    /// <exception cref="ArgumentException">Thrown when users don't exist</exception>
    Task UnfollowUserAsync(Guid followerId, Guid followeeId);

    /// <summary>
    /// Searches for users by username with business logic for filtering
    /// </summary>
    /// <param name="searchTerm">Search term to match usernames</param>
    /// <param name="limit">Maximum number of results (default: 20)</param>
    /// <returns>Collection of matching users</returns>
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 20);
}
