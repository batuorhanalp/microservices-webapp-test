using Microsoft.Extensions.Logging;
using WebApp.Core.Entities;
using WebApp.Core.Interfaces;

namespace WebApp.Application.Services;

/// <summary>
/// User service implementation handling business logic for user operations
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User> CreateUserAsync(string email, string username, string displayName)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        _logger.LogInformation("Creating new user with email: {Email}, username: {Username}", email, username);

        // Check if email is already taken
        if (await _userRepository.IsEmailTakenAsync(email))
        {
            _logger.LogWarning("Attempted to create user with taken email: {Email}", email);
            throw new ArgumentException("Email is already taken", nameof(email));
        }

        // Check if username is already taken
        if (await _userRepository.IsUsernameTakenAsync(username))
        {
            _logger.LogWarning("Attempted to create user with taken username: {Username}", username);
            throw new ArgumentException("Username is already taken", nameof(username));
        }

        // Create new user
        var user = new User(email, username, displayName);
        
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created user with ID: {UserId}", user.Id);
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("GetUserByIdAsync called with empty GUID");
            return null;
        }

        _logger.LogDebug("Retrieving user with ID: {UserId}", userId);
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User> UpdateUserProfileAsync(Guid userId, string displayName, string? bio = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        _logger.LogInformation("Updating profile for user ID: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for profile update: {UserId}", userId);
            throw new ArgumentException("User not found", nameof(userId));
        }

        // Update user profile
        user.UpdateProfile(displayName, bio ?? string.Empty, user.Website, user.Location);
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully updated profile for user ID: {UserId}", userId);
        return user;
    }

    public async Task FollowUserAsync(Guid followerId, Guid followeeId)
    {
        if (followerId == Guid.Empty)
            throw new ArgumentException("Follower ID cannot be empty", nameof(followerId));
        if (followeeId == Guid.Empty)
            throw new ArgumentException("Followee ID cannot be empty", nameof(followeeId));
        if (followerId == followeeId)
            throw new ArgumentException("A user cannot follow themselves");

        _logger.LogInformation("Processing follow request: {FollowerId} -> {FolloweeId}", followerId, followeeId);

        // Verify both users exist
        var follower = await _userRepository.GetByIdAsync(followerId);
        if (follower == null)
        {
            _logger.LogWarning("Follower not found: {FollowerId}", followerId);
            throw new ArgumentException("Follower not found", nameof(followerId));
        }

        var followee = await _userRepository.GetByIdAsync(followeeId);
        if (followee == null)
        {
            _logger.LogWarning("User to follow not found: {FolloweeId}", followeeId);
            throw new ArgumentException("User to follow not found", nameof(followeeId));
        }

        // For now, just validate users exist and save - follow logic can be implemented later
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully validated follow request: {FollowerId} -> {FolloweeId}", followerId, followeeId);
    }

    public async Task UnfollowUserAsync(Guid followerId, Guid followeeId)
    {
        if (followerId == Guid.Empty)
            throw new ArgumentException("Follower ID cannot be empty", nameof(followerId));
        if (followeeId == Guid.Empty)
            throw new ArgumentException("Followee ID cannot be empty", nameof(followeeId));

        _logger.LogInformation("Processing unfollow request: {FollowerId} -x-> {FolloweeId}", followerId, followeeId);

        // Verify both users exist
        var follower = await _userRepository.GetByIdAsync(followerId);
        if (follower == null)
        {
            _logger.LogWarning("Follower not found for unfollow: {FollowerId}", followerId);
            throw new ArgumentException("Follower not found", nameof(followerId));
        }

        var followee = await _userRepository.GetByIdAsync(followeeId);
        if (followee == null)
        {
            _logger.LogWarning("User to unfollow not found: {FolloweeId}", followeeId);
            throw new ArgumentException("User to unfollow not found", nameof(followeeId));
        }

        // For now, just validate users exist and save - unfollow logic can be implemented later
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully validated unfollow request: {FollowerId} -x-> {FolloweeId}", followerId, followeeId);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 20)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            _logger.LogDebug("SearchUsersAsync called with empty search term");
            return Enumerable.Empty<User>();
        }

        if (limit <= 0)
            limit = 20;

        _logger.LogDebug("Searching users with term: {SearchTerm}, limit: {Limit}", searchTerm, limit);

        var users = await _userRepository.SearchByUsernameAsync(searchTerm, limit);
        
        _logger.LogDebug("Found {UserCount} users matching search term: {SearchTerm}", users.Count(), searchTerm);
        return users;
    }
}
