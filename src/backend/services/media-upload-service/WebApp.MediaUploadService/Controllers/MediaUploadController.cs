using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Common.DTOs;
using WebApp.Common.Interfaces;

namespace WebApp.MediaUploadService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaUploadController : ControllerBase
{
    private readonly IMediaUploadService _mediaUploadService;
    private readonly ILogger<MediaUploadController> _logger;

    public MediaUploadController(IMediaUploadService mediaUploadService, ILogger<MediaUploadController> logger)
    {
        _mediaUploadService = mediaUploadService ?? throw new ArgumentNullException(nameof(mediaUploadService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Upload a single media file
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(MediaUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
    public async Task<ActionResult<MediaUploadResponse>> UploadAsync([FromForm] MediaUploadRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserIdFromClaims();
            var response = await _mediaUploadService.UploadAsync(request, userId);

            _logger.LogInformation("File uploaded successfully: {FileName} by user {UserId}", 
                response.FileName, userId);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Upload validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Upload operation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file upload");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while uploading the file" });
        }
    }

    /// <summary>
    /// Upload multiple media files at once
    /// </summary>
    [HttpPost("upload/bulk")]
    [ProducesResponseType(typeof(BulkMediaUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [RequestSizeLimit(500 * 1024 * 1024)] // 500MB limit for bulk uploads
    [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
    public async Task<ActionResult<BulkMediaUploadResponse>> BulkUploadAsync([FromForm] BulkMediaUploadRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserIdFromClaims();
            var response = await _mediaUploadService.BulkUploadAsync(request, userId);

            _logger.LogInformation("Bulk upload completed: {SuccessCount}/{TotalCount} files uploaded by user {UserId}", 
                response.SuccessCount, response.TotalFiles, userId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Bulk upload validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during bulk file upload");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while uploading files" });
        }
    }

    /// <summary>
    /// Get media file by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MediaUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaUploadResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var media = await _mediaUploadService.GetByIdAsync(id);
            if (media == null)
            {
                return NotFound(new { error = $"Media file with ID {id} not found" });
            }

            return Ok(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving media file {MediaId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the media file" });
        }
    }

    /// <summary>
    /// Search and filter media files
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(MediaSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaSearchResponse>> SearchAsync([FromBody] MediaSearchRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // If no user ID specified in request, default to current user
            if (request.UserId == null)
            {
                request.UserId = GetUserIdFromClaims();
            }

            var response = await _mediaUploadService.SearchAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Media search validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching media files");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while searching media files" });
        }
    }

    /// <summary>
    /// Update media metadata
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MediaUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MediaUploadResponse>> UpdateAsync(Guid id, [FromBody] UpdateMediaRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserIdFromClaims();
            var response = await _mediaUploadService.UpdateAsync(id, request, userId);

            _logger.LogInformation("Media metadata updated: {MediaId} by user {UserId}", id, userId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Media update validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You don't have permission to update this media file");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = $"Media file with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media file {MediaId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while updating the media file" });
        }
    }

    /// <summary>
    /// Delete a media file
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var success = await _mediaUploadService.DeleteAsync(id, userId);

            if (!success)
            {
                return NotFound(new { error = $"Media file with ID {id} not found" });
            }

            _logger.LogInformation("Media file deleted: {MediaId} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You don't have permission to delete this media file");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media file {MediaId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while deleting the media file" });
        }
    }

    /// <summary>
    /// Associate temporary media with a post
    /// </summary>
    [HttpPost("{id:guid}/associate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssociateWithPostAsync(Guid id, [FromBody] AssociateMediaRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserIdFromClaims();
            var success = await _mediaUploadService.AssociateWithPostAsync(id, request.PostId, userId);

            if (!success)
            {
                return NotFound(new { error = $"Media file with ID {id} not found" });
            }

            _logger.LogInformation("Media associated with post: {MediaId} -> {PostId} by user {UserId}", 
                id, request.PostId, userId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Media association validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating media {MediaId} with post {PostId}", id, request.PostId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while associating the media with post" });
        }
    }

    /// <summary>
    /// Get media storage statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(MediaStorageStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<MediaStorageStats>> GetStorageStatsAsync([FromQuery] bool? includeGlobal = false)
    {
        try
        {
            var userId = includeGlobal == true ? (Guid?)null : GetUserIdFromClaims();
            var stats = await _mediaUploadService.GetStorageStatsAsync(userId);

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving storage statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving storage statistics" });
        }
    }

    /// <summary>
    /// Validate a file before upload (without actually uploading)
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ValidationResult>> ValidateFileAsync([FromForm] IFormFile file)
    {
        try
        {
            if (file == null)
            {
                return BadRequest(new { error = "No file provided" });
            }

            var result = await _mediaUploadService.ValidateFileAsync(file);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while validating the file" });
        }
    }

    /// <summary>
    /// Clean up temporary files (Admin only)
    /// </summary>
    [HttpPost("cleanup")]
    [ProducesResponseType(typeof(CleanupResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CleanupResult>> CleanupTemporaryFilesAsync([FromBody] CleanupRequest? request = null)
    {
        try
        {
            var maxAge = request?.MaxAgeHours != null 
                ? TimeSpan.FromHours(request.MaxAgeHours.Value)
                : TimeSpan.FromHours(24); // Default 24 hours

            var deletedCount = await _mediaUploadService.CleanupTemporaryFilesAsync(maxAge);

            _logger.LogInformation("Cleanup completed: {DeletedCount} temporary files removed", deletedCount);

            return Ok(new CleanupResult 
            { 
                DeletedFilesCount = deletedCount,
                MaxAge = maxAge,
                CompletedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during temporary files cleanup");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred during cleanup" });
        }
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}

// Additional DTOs for controller-specific requests
public class AssociateMediaRequest
{
    public Guid PostId { get; set; }
}

public class CleanupRequest
{
    public int? MaxAgeHours { get; set; }
}

public class CleanupResult
{
    public int DeletedFilesCount { get; set; }
    public TimeSpan MaxAge { get; set; }
    public DateTime CompletedAt { get; set; }
}
