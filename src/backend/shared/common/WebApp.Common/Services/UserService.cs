using Microsoft.Extensions.Logging;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.Common.Services;

/// <summary>
/// Service implementation for user-related business operations.
/// Handles user management with proper validation and business rules enforcement.
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
        _logger.LogInformation("Creating user with email {Email} and username {Username}", email, username);

        // Input validation
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        // Check if email is already taken
        if (await _userRepository.IsEmailTakenAsync(email))
            throw new ArgumentException("Email address is already in use", nameof(email));

        // Check if username is already taken
        if (await _userRepository.IsUsernameTakenAsync(username))
            throw new ArgumentException("Username is already in use", nameof(username));

        // Create user
        var user = new User(email, username, displayName);

        // Save to repository
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created user {UserId} with username {Username}", user.Id, username);
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        _logger.LogDebug("Retrieving user {UserId}", userId);

        if (userId == Guid.Empty)
            return null;

        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User> UpdateUserProfileAsync(Guid userId, string displayName, string? bio = null)
    {
        _logger.LogInformation("Updating profile for user {UserId}", userId);

        // Input validation
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        // Get user
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        // Update profile
        user.UpdateProfile(displayName, bio ?? string.Empty, string.Empty, string.Empty);

        // Save changes
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully updated profile for user {UserId}", userId);
        return user;
    }

    public async Task FollowUserAsync(Guid followerId, Guid followeeId)
    {
        _logger.LogInformation("User {FollowerId} attempting to follow user {FolloweeId}", followerId, followeeId);

        // Input validation
        if (followerId == Guid.Empty)
            throw new ArgumentException("Follower ID cannot be empty", nameof(followerId));

        if (followeeId == Guid.Empty)
            throw new ArgumentException("Followee ID cannot be empty", nameof(followeeId));

        if (followerId == followeeId)
            throw new ArgumentException("Users cannot follow themselves");

        // Verify both users exist
        var follower = await _userRepository.GetByIdAsync(followerId);
        if (follower == null)
            throw new ArgumentException("Follower user not found", nameof(followerId));

        var followee = await _userRepository.GetByIdAsync(followeeId);
        if (followee == null)
            throw new ArgumentException("Followee user not found", nameof(followeeId));

        // Create follow relationship
        var follow = new Follow(followerId, followeeId, followee.IsPrivate);

        // Add to follower's following collection
        follower.Following.Add(follow);

        // Update follower
        _userRepository.Update(follower);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created follow relationship: {FollowerId} -> {FolloweeId}", followerId, followeeId);
    }

    public async Task UnfollowUserAsync(Guid followerId, Guid followeeId)
    {
        _logger.LogInformation("User {FollowerId} attempting to unfollow user {FolloweeId}", followerId, followeeId);

        // Input validation
        if (followerId == Guid.Empty)
            throw new ArgumentException("Follower ID cannot be empty", nameof(followerId));

        if (followeeId == Guid.Empty)
            throw new ArgumentException("Followee ID cannot be empty", nameof(followeeId));

        // Get follower
        var follower = await _userRepository.GetByIdAsync(followerId);
        if (follower == null)
            throw new ArgumentException("Follower user not found", nameof(followerId));

        // Find and remove follow relationship
        var follow = follower.Following.FirstOrDefault(f => f.FolloweeId == followeeId);
        if (follow != null)
        {
            follower.Following.Remove(follow);
            _userRepository.Update(follower);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully removed follow relationship: {FollowerId} -> {FolloweeId}", followerId, followeeId);
        }
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 20)
    {
        _logger.LogDebug("Searching users with term {SearchTerm} and limit {Limit}", searchTerm, limit);

        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<User>();

        if (limit <= 0)
            limit = 20;

        return await _userRepository.SearchByUsernameAsync(searchTerm, limit);
    }
}
