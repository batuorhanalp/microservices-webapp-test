using Microsoft.AspNetCore.Mvc;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.PostService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<PostsController> _logger;

    public PostsController(IPostService postService, ILogger<PostsController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(Guid id, [FromQuery] Guid? viewerId = null)
    {
        try
        {
            var post = await _postService.GetPostByIdAsync(id, viewerId);
            if (post == null)
                return NotFound();

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving post {PostId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("text")]
    public async Task<ActionResult<Post>> CreateTextPost([FromBody] CreateTextPostRequest request)
    {
        try
        {
            var post = await _postService.CreateTextPostAsync(
                request.AuthorId, 
                request.Content, 
                request.Visibility);
            
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating text post");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating text post");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("media")]
    public async Task<ActionResult<Post>> CreateMediaPost([FromBody] CreateMediaPostRequest request)
    {
        try
        {
            var post = await _postService.CreateMediaPostAsync(
                request.AuthorId, 
                request.Content, 
                request.MediaAttachments,
                request.Visibility);
            
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating media post");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating media post");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("reply")]
    public async Task<ActionResult<Post>> CreateReply([FromBody] CreateReplyRequest request)
    {
        try
        {
            var post = await _postService.CreateReplyAsync(
                request.AuthorId, 
                request.ParentPostId, 
                request.Content, 
                request.Visibility);
            
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
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

    [HttpPut("{id}/content")]
    public async Task<ActionResult<Post>> UpdatePostContent(Guid id, [FromBody] UpdateContentRequest request)
    {
        try
        {
            var post = await _postService.UpdatePostContentAsync(id, request.AuthorId, request.NewContent);
            return Ok(post);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating post content");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post content for post {PostId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id, [FromQuery] Guid authorId)
    {
        try
        {
            var success = await _postService.DeletePostAsync(id, authorId);
            if (success)
                return NoContent();
            
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for deleting post");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("author/{authorId}")]
    public async Task<ActionResult<IEnumerable<Post>>> GetPostsByAuthor(
        Guid authorId, 
        [FromQuery] Guid? viewerId = null,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            var posts = await _postService.GetPostsByAuthorAsync(authorId, viewerId, limit, offset);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posts by author {AuthorId}", authorId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("feed/{userId}")]
    public async Task<ActionResult<IEnumerable<Post>>> GetUserFeed(
        Guid userId,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            var posts = await _postService.GetUserFeedAsync(userId, limit, offset);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feed for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("public")]
    public async Task<ActionResult<IEnumerable<Post>>> GetPublicTimeline(
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            var posts = await _postService.GetPublicTimelineAsync(limit, offset);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public timeline");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Post>>> SearchPosts(
        [FromQuery] string searchTerm,
        [FromQuery] Guid? viewerId = null,
        [FromQuery] int limit = 20)
    {
        try
        {
            var posts = await _postService.SearchPostsAsync(searchTerm, viewerId, limit);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching posts with term {SearchTerm}", searchTerm);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("media")]
    public async Task<ActionResult<IEnumerable<Post>>> GetMediaPosts(
        [FromQuery] Guid? viewerId = null,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            var posts = await _postService.GetMediaPostsAsync(viewerId, limit, offset);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving media posts");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{parentId}/replies")]
    public async Task<ActionResult<IEnumerable<Post>>> GetPostReplies(
        Guid parentId,
        [FromQuery] Guid? viewerId = null,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            var replies = await _postService.GetPostRepliesAsync(parentId, viewerId, limit, offset);
            return Ok(replies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving replies for post {PostId}", parentId);
            return StatusCode(500, "Internal server error");
        }
    }
}

// DTOs for requests
public record CreateTextPostRequest(Guid AuthorId, string Content, PostVisibility Visibility = PostVisibility.Public);
public record CreateMediaPostRequest(Guid AuthorId, string? Content, IEnumerable<MediaAttachment> MediaAttachments, PostVisibility Visibility = PostVisibility.Public);
public record CreateReplyRequest(Guid AuthorId, Guid ParentPostId, string Content, PostVisibility Visibility = PostVisibility.Public);
public record UpdateContentRequest(Guid AuthorId, string NewContent);
