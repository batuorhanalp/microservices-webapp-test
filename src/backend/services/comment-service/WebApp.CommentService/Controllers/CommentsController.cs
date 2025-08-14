using Microsoft.AspNetCore.Mvc;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.CommentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Comment>> GetComment(Guid id, [FromQuery] Guid? viewerId = null)
    {
        try
        {
            var comment = await _commentService.GetCommentByIdAsync(id, viewerId);
            if (comment == null)
                return NotFound();

            return Ok(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comment {CommentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Comment>> CreateComment([FromBody] CreateCommentRequest request)
    {
        try
        {
            var comment = await _commentService.CreateCommentAsync(
                request.AuthorId, 
                request.PostId, 
                request.Content);
            
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating comment");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("reply")]
    public async Task<ActionResult<Comment>> CreateReply([FromBody] CreateReplyRequest request)
    {
        try
        {
            var comment = await _commentService.CreateReplyAsync(
                request.AuthorId, 
                request.ParentCommentId, 
                request.Content);
            
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating reply");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reply");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Comment>> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request)
    {
        try
        {
            var comment = await _commentService.UpdateCommentAsync(id, request.AuthorId, request.NewContent);
            return Ok(comment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating comment");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id, [FromQuery] Guid authorId)
    {
        try
        {
            var success = await _commentService.DeleteCommentAsync(id, authorId);
            if (success)
                return NoContent();
            
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for deleting comment");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("post/{postId}")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetPostComments(
        Guid postId,
        [FromQuery] Guid? viewerId = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            var comments = await _commentService.GetPostCommentsAsync(postId, viewerId, limit, offset);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for post {PostId}", postId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetUserComments(
        Guid userId,
        [FromQuery] Guid? viewerId = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            var comments = await _commentService.GetUserCommentsAsync(userId, viewerId, limit, offset);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{parentCommentId}/replies")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentReplies(
        Guid parentCommentId,
        [FromQuery] Guid? viewerId = null,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            var replies = await _commentService.GetCommentRepliesAsync(parentCommentId, viewerId, limit, offset);
            return Ok(replies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving replies for comment {CommentId}", parentCommentId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("post/{postId}/count")]
    public async Task<ActionResult<int>> GetPostCommentCount(Guid postId)
    {
        try
        {
            var count = await _commentService.GetPostCommentCountAsync(postId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comment count for post {PostId}", postId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("user/{userId}/count")]
    public async Task<ActionResult<int>> GetUserCommentCount(Guid userId)
    {
        try
        {
            var count = await _commentService.GetUserCommentCountAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comment count for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{parentCommentId}/replies/count")]
    public async Task<ActionResult<int>> GetCommentReplyCount(Guid parentCommentId)
    {
        try
        {
            var count = await _commentService.GetCommentReplyCountAsync(parentCommentId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reply count for comment {CommentId}", parentCommentId);
            return StatusCode(500, "Internal server error");
        }
    }
}

// DTOs for requests
public record CreateCommentRequest(Guid AuthorId, Guid PostId, string Content);
public record CreateReplyRequest(Guid AuthorId, Guid ParentCommentId, string Content);
public record UpdateCommentRequest(Guid AuthorId, string NewContent);
