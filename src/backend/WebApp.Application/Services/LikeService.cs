using Microsoft.Extensions.Logging;
using WebApp.Core.Entities;
using WebApp.Core.Interfaces;

namespace WebApp.Application.Services;

/// <summary>
/// Service implementation for managing like-related business logic operations.
/// Handles like/unlike operations, like status checking, and like metrics with proper 
/// validation, authorization, and business rule enforcement.
/// </summary>
public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ILogger<LikeService> _logger;

    public LikeService(ILikeRepository likeRepository, IUserRepository userRepository, 
        IPostRepository postRepository, ILogger<LikeService> logger)
    {
        _likeRepository = likeRepository ?? throw new ArgumentNullException(nameof(likeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Like> LikePostAsync(Guid userId, Guid postId)
    {
        _logger.LogInformation("Processing like request: User {UserId} -> Post {PostId}", userId, postId);

        // Input validation
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(postId));

        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Like attempt failed - User not found: {UserId}", userId);
            throw new ArgumentException("User not found", nameof(userId));
        }

        // Verify post exists
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            _logger.LogWarning("Like attempt failed - Post not found: {PostId}", postId);
            throw new ArgumentException("Post not found", nameof(postId));
        }

        // Check if like already exists
        var existingLike = await _likeRepository.ExistsAsync(userId, postId);
        if (existingLike)
        {
            _logger.LogWarning("Like attempt failed - User {UserId} has already liked post {PostId}", userId, postId);
            throw new ArgumentException("User has already liked this post");
        }

        // Create new like
        var like = new Like(userId, postId);

        // Save like
        await _likeRepository.AddAsync(like);
        await _likeRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created like: User {UserId} -> Post {PostId}, Like ID: {LikeId}", 
            userId, postId, like.Id);
        
        return like;
    }

    public async Task UnlikePostAsync(Guid userId, Guid postId)
    {
        _logger.LogInformation("Processing unlike request: User {UserId} -x-> Post {PostId}", userId, postId);

        // Input validation
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(postId));

        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Unlike attempt failed - User not found: {UserId}", userId);
            throw new ArgumentException("User not found", nameof(userId));
        }

        // Verify post exists
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            _logger.LogWarning("Unlike attempt failed - Post not found: {PostId}", postId);
            throw new ArgumentException("Post not found", nameof(postId));
        }

        // Check if like exists
        var existingLike = await _likeRepository.ExistsAsync(userId, postId);
        if (!existingLike)
        {
            _logger.LogWarning("Unlike attempt failed - User {UserId} has not liked post {PostId}", userId, postId);
            throw new ArgumentException("User has not liked this post");
        }

        // Remove like
        await _likeRepository.DeleteByUserAndPostAsync(userId, postId);
        await _likeRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully removed like: User {UserId} -x-> Post {PostId}", userId, postId);
    }

    public async Task<bool> HasUserLikedPostAsync(Guid userId, Guid postId)
    {
        // Handle empty IDs gracefully (return false instead of throwing)
        if (userId == Guid.Empty || postId == Guid.Empty)
        {
            _logger.LogDebug("HasUserLikedPostAsync called with empty ID(s): UserId={UserId}, PostId={PostId}", 
                userId, postId);
            return false;
        }

        _logger.LogDebug("Checking like status: User {UserId} -> Post {PostId}", userId, postId);

        var hasLiked = await _likeRepository.ExistsAsync(userId, postId);
        
        _logger.LogDebug("Like status result: User {UserId} -> Post {PostId} = {HasLiked}", 
            userId, postId, hasLiked);
        
        return hasLiked;
    }

    public async Task<IEnumerable<Like>> GetPostLikesAsync(Guid postId, int limit = 50, int offset = 0)
    {
        if (postId == Guid.Empty)
        {
            _logger.LogDebug("Empty post ID provided for GetPostLikesAsync, returning empty list");
            return Enumerable.Empty<Like>();
        }

        // Normalize pagination parameters
        if (limit <= 0) limit = 50;
        if (offset < 0) offset = 0;

        _logger.LogDebug("Retrieving likes for post {PostId} with limit {Limit} and offset {Offset}", 
            postId, limit, offset);

        var likes = await _likeRepository.GetByPostAsync(postId, limit, offset);

        _logger.LogDebug("Retrieved {LikeCount} likes for post {PostId}", likes.Count(), postId);
        return likes;
    }

    public async Task<IEnumerable<Like>> GetUserLikesAsync(Guid userId, int limit = 50, int offset = 0)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogDebug("Empty user ID provided for GetUserLikesAsync, returning empty list");
            return Enumerable.Empty<Like>();
        }

        // Normalize pagination parameters
        if (limit <= 0) limit = 50;
        if (offset < 0) offset = 0;

        _logger.LogDebug("Retrieving likes by user {UserId} with limit {Limit} and offset {Offset}", 
            userId, limit, offset);

        var likes = await _likeRepository.GetByUserAsync(userId, limit, offset);

        _logger.LogDebug("Retrieved {LikeCount} likes by user {UserId}", likes.Count(), userId);
        return likes;
    }

    public async Task<int> GetPostLikeCountAsync(Guid postId)
    {
        if (postId == Guid.Empty)
        {
            _logger.LogDebug("Empty post ID provided for GetPostLikeCountAsync, returning 0");
            return 0;
        }

        _logger.LogDebug("Retrieving like count for post {PostId}", postId);

        var count = await _likeRepository.GetCountByPostAsync(postId);

        _logger.LogDebug("Post {PostId} has {LikeCount} likes", postId, count);
        return count;
    }

    public async Task<int> GetUserLikeCountAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogDebug("Empty user ID provided for GetUserLikeCountAsync, returning 0");
            return 0;
        }

        _logger.LogDebug("Retrieving like count for user {UserId}", userId);

        var count = await _likeRepository.GetCountByUserAsync(userId);

        _logger.LogDebug("User {UserId} has made {LikeCount} likes", userId, count);
        return count;
    }

    public async Task<IEnumerable<User>> GetUsersWhoLikedPostAsync(Guid postId, int limit = 50, int offset = 0)
    {
        if (postId == Guid.Empty)
        {
            _logger.LogDebug("Empty post ID provided for GetUsersWhoLikedPostAsync, returning empty list");
            return Enumerable.Empty<User>();
        }

        // Normalize pagination parameters
        if (limit <= 0) limit = 50;
        if (offset < 0) offset = 0;

        _logger.LogDebug("Retrieving users who liked post {PostId} with limit {Limit} and offset {Offset}", 
            postId, limit, offset);

        var users = await _likeRepository.GetUsersByPostAsync(postId, limit, offset);

        _logger.LogDebug("Retrieved {UserCount} users who liked post {PostId}", users.Count(), postId);
        return users;
    }
}
