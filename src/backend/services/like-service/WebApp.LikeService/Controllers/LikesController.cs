using Microsoft.AspNetCore.Mvc;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.LikeService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;
    private readonly ILogger<LikesController> _logger;

    public LikesController(ILikeService likeService, ILogger<LikesController> logger)
    {
        _likeService = likeService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Like>> LikePost([FromBody] LikePostRequest request)
    {
        try
        {
            var like = await _likeService.LikePostAsync(request.UserId, request.PostId);
            return CreatedAtAction(nameof(GetLike), new { userId = like.UserId, postId = like.PostId }, like);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for liking post");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking post {PostId} by user {UserId}", request.PostId, request.UserId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{userId}/{postId}")]
    public async Task<IActionResult> UnlikePost(Guid userId, Guid postId)
    {
        try
        {
            await _likeService.UnlikePostAsync(userId, postId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for unliking post");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking post {PostId} by user {UserId}", postId, userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{userId}/{postId}")]
    public async Task<ActionResult<Like>> GetLike(Guid userId, Guid postId)
    {
        try
        {
            var hasLiked = await _likeService.HasUserLikedPostAsync(userId, postId);
            if (!hasLiked)
                return NotFound();

            // For this endpoint, we could return a simple status or the like object
            // Since we need to return a Like object, we need to get the actual like
            var likes = await _likeService.GetPostLikesAsync(postId, 1, 0);
            var like = likes.FirstOrDefault(l => l.UserId == userId);
            
            return like != null ? Ok(like) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking like status for post {PostId} by user {UserId}", postId, userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{userId}/{postId}/status")]
    public async Task<ActionResult<bool>> GetLikeStatus(Guid userId, Guid postId)
    {
        try
        {
            var hasLiked = await _likeService.HasUserLikedPostAsync(userId, postId);
            return Ok(hasLiked);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking like status for post {PostId} by user {UserId}", postId, userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("post/{postId}")]
    public async Task<ActionResult<IEnumerable<Like>>> GetPostLikes(
        Guid postId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            var likes = await _likeService.GetPostLikesAsync(postId, limit, offset);
            return Ok(likes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving likes for post {PostId}", postId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Like>>> GetUserLikes(
        Guid userId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            var likes = await _likeService.GetUserLikesAsync(userId, limit, offset);
            return Ok(likes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving likes for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("post/{postId}/count")]
    public async Task<ActionResult<int>> GetPostLikeCount(Guid postId)
    {
        try
        {
            var count = await _likeService.GetPostLikeCountAsync(postId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving like count for post {PostId}", postId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("user/{userId}/count")]
    public async Task<ActionResult<int>> GetUserLikeCount(Guid userId)
    {
        try
        {
            var count = await _likeService.GetUserLikeCountAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving like count for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("post/{postId}/users")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersWhoLikedPost(
        Guid postId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            var users = await _likeService.GetUsersWhoLikedPostAsync(postId, limit, offset);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users who liked post {PostId}", postId);
            return StatusCode(500, "Internal server error");
        }
    }
}

// DTOs for requests
public record LikePostRequest(Guid UserId, Guid PostId);
