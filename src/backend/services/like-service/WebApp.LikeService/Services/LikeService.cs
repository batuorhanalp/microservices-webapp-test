using Microsoft.Extensions.Logging;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.LikeService.Services;

/// <summary>
/// Service implementation for like-related business operations.
/// Handles liking/unliking posts and retrieving like information with proper validation
/// and business rules enforcement.
/// </summary>
public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ILogger<LikeService> _logger;

    public LikeService(
        ILikeRepository likeRepository, 
        IUserRepository userRepository, 
        IPostRepository postRepository,
        ILogger<LikeService> logger)
    {
        _likeRepository = likeRepository ?? throw new ArgumentNullException(nameof(likeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Like> LikePostAsync(Guid userId, Guid postId)
    {
        _logger.LogInformation("User {UserId} attempting to like post {PostId}", userId, postId);

        // Input validation
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(postId));

        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        // Verify post exists
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new ArgumentException("Post not found", nameof(postId));

        // Check if like already exists
        var existingLike = await _likeRepository.GetByUserAndPostAsync(userId, postId);
        if (existingLike != null)
            throw new ArgumentException("User has already liked this post");

        // Business rule: Users cannot like their own posts (optional - remove if not desired)
        if (post.AuthorId == userId)
            throw new ArgumentException("Users cannot like their own posts");

        // Create like
        var like = new Like(userId, postId);

        // Save to repository
        await _likeRepository.AddAsync(like);
        await _likeRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created like {LikeId} for post {PostId} by user {UserId}", 
            like.Id, postId, userId);
        return like;
    }

    public async Task UnlikePostAsync(Guid userId, Guid postId)
    {
        _logger.LogInformation("User {UserId} attempting to unlike post {PostId}", userId, postId);

        // Input validation
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(postId));

        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        // Verify post exists
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new ArgumentException("Post not found", nameof(postId));

        // Check if like exists
        var existingLike = await _likeRepository.GetByUserAndPostAsync(userId, postId);
        if (existingLike == null)
            throw new ArgumentException("Like not found - user has not liked this post");

        // Remove like
        await _likeRepository.DeleteByUserAndPostAsync(userId, postId);
        await _likeRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully removed like for post {PostId} by user {UserId}", postId, userId);
    }

    public async Task<bool> HasUserLikedPostAsync(Guid userId, Guid postId)
    {
        _logger.LogDebug("Checking if user {UserId} has liked post {PostId}", userId, postId);

        // Input validation
        if (userId == Guid.Empty)
            return false;

        if (postId == Guid.Empty)
            return false;

        return await _likeRepository.ExistsAsync(userId, postId);
    }

    public async Task<IEnumerable<Like>> GetPostLikesAsync(Guid postId, int limit = 50, int offset = 0)
    {
        _logger.LogDebug("Retrieving likes for post {PostId} with limit {Limit} and offset {Offset}", 
            postId, limit, offset);

        // Input validation
        if (postId == Guid.Empty)
            return Enumerable.Empty<Like>();

        if (limit <= 0)
            limit = 50;

        if (offset < 0)
            offset = 0;

        return await _likeRepository.GetByPostAsync(postId, limit, offset);
    }

    public async Task<IEnumerable<Like>> GetUserLikesAsync(Guid userId, int limit = 50, int offset = 0)
    {
        _logger.LogDebug("Retrieving likes for user {UserId} with limit {Limit} and offset {Offset}", 
            userId, limit, offset);

        // Input validation
        if (userId == Guid.Empty)
            return Enumerable.Empty<Like>();

        if (limit <= 0)
            limit = 50;

        if (offset < 0)
            offset = 0;

        return await _likeRepository.GetByUserAsync(userId, limit, offset);
    }

    public async Task<int> GetPostLikeCountAsync(Guid postId)
    {
        _logger.LogDebug("Retrieving like count for post {PostId}", postId);

        // Input validation
        if (postId == Guid.Empty)
            return 0;

        return await _likeRepository.GetCountByPostAsync(postId);
    }

    public async Task<int> GetUserLikeCountAsync(Guid userId)
    {
        _logger.LogDebug("Retrieving like count for user {UserId}", userId);

        // Input validation
        if (userId == Guid.Empty)
            return 0;

        return await _likeRepository.GetCountByUserAsync(userId);
    }

    public async Task<IEnumerable<User>> GetUsersWhoLikedPostAsync(Guid postId, int limit = 50, int offset = 0)
    {
        _logger.LogDebug("Retrieving users who liked post {PostId} with limit {Limit} and offset {Offset}", 
            postId, limit, offset);

        // Input validation
        if (postId == Guid.Empty)
            return Enumerable.Empty<User>();

        if (limit <= 0)
            limit = 50;

        if (offset < 0)
            offset = 0;

        return await _likeRepository.GetUsersByPostAsync(postId, limit, offset);
    }
}
