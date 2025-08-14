using WebApp.Core.Entities;

namespace WebApp.Core.Interfaces;

/// <summary>
/// Repository interface for User entity operations
/// Defines the contract for user data access
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets a user by their username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Adds a new user to the repository
    /// </summary>
    /// <param name="user">User entity to add</param>
    Task AddAsync(User user);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">User entity to update</param>
    void Update(User user);

    /// <summary>
    /// Deletes a user by their ID
    /// </summary>
    /// <param name="id">User ID to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets all followers for a specific user
    /// </summary>
    /// <param name="userId">User ID to get followers for</param>
    /// <returns>List of users who follow the specified user</returns>
    Task<IEnumerable<User>> GetFollowersAsync(Guid userId);

    /// <summary>
    /// Gets all users that a specific user is following
    /// </summary>
    /// <param name="userId">User ID to get following list for</param>
    /// <returns>List of users that the specified user follows</returns>
    Task<IEnumerable<User>> GetFollowingAsync(Guid userId);

    /// <summary>
    /// Searches for users by username (partial match)
    /// </summary>
    /// <param name="searchTerm">Username search term</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <returns>List of users matching the search criteria</returns>
    Task<IEnumerable<User>> SearchByUsernameAsync(string searchTerm, int limit = 20);

    /// <summary>
    /// Checks if an email address is already taken
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>True if email is taken, false otherwise</returns>
    Task<bool> IsEmailTakenAsync(string email);

    /// <summary>
    /// Checks if a username is already taken
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <returns>True if username is taken, false otherwise</returns>
    Task<bool> IsUsernameTakenAsync(string username);

    /// <summary>
    /// Saves all changes to the underlying data store
    /// </summary>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync();
}
