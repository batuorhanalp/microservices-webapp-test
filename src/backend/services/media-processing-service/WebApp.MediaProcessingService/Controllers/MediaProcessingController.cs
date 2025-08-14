using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Common.DTOs;
using WebApp.Common.Interfaces;

namespace WebApp.MediaProcessingService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaProcessingController : ControllerBase
{
    private readonly IMediaProcessingService _mediaProcessingService;
    private readonly ILogger<MediaProcessingController> _logger;

    public MediaProcessingController(IMediaProcessingService mediaProcessingService, ILogger<MediaProcessingController> logger)
    {
        _mediaProcessingService = mediaProcessingService ?? throw new ArgumentNullException(nameof(mediaProcessingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Submit a media file for processing
    /// </summary>
    [HttpPost("jobs")]
    [ProducesResponseType(typeof(MediaProcessingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaProcessingResponse>> SubmitProcessingJobAsync([FromBody] MediaProcessingRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _mediaProcessingService.SubmitProcessingJobAsync(request);
            
            _logger.LogInformation("Processing job submitted: {JobId} for media {MediaId} (Type: {ProcessingType})", 
                response.JobId, response.MediaId, response.ProcessingType);

            return AcceptedAtAction(nameof(GetProcessingStatusAsync), new { jobId = response.JobId }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Processing job validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Processing job operation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error submitting processing job for media {MediaId}", request.MediaId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while submitting the processing job" });
        }
    }

    /// <summary>
    /// Get processing job status
    /// </summary>
    [HttpGet("jobs/{jobId:guid}")]
    [ProducesResponseType(typeof(MediaProcessingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaProcessingResponse>> GetProcessingStatusAsync(Guid jobId)
    {
        try
        {
            var response = await _mediaProcessingService.GetProcessingStatusAsync(jobId);
            if (response == null)
            {
                return NotFound(new { error = $"Processing job with ID {jobId} not found" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving processing job status {JobId}", jobId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the processing job status" });
        }
    }

    /// <summary>
    /// Cancel a processing job
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelProcessingJobAsync(Guid jobId)
    {
        try
        {
            var success = await _mediaProcessingService.CancelProcessingJobAsync(jobId);
            if (!success)
            {
                return NotFound(new { error = $"Processing job with ID {jobId} not found or cannot be cancelled" });
            }

            _logger.LogInformation("Processing job cancelled: {JobId}", jobId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot cancel processing job {JobId}: {Message}", jobId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling processing job {JobId}", jobId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while cancelling the processing job" });
        }
    }

    /// <summary>
    /// Retry a failed processing job
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/retry")]
    [ProducesResponseType(typeof(MediaProcessingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaProcessingResponse>> RetryProcessingJobAsync(Guid jobId)
    {
        try
        {
            var response = await _mediaProcessingService.RetryProcessingJobAsync(jobId);
            
            _logger.LogInformation("Processing job retry submitted: {JobId}", jobId);
            return AcceptedAtAction(nameof(GetProcessingStatusAsync), new { jobId = response.JobId }, response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = $"Processing job with ID {jobId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot retry processing job {JobId}: {Message}", jobId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying processing job {JobId}", jobId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrying the processing job" });
        }
    }

    /// <summary>
    /// Get all processing jobs for a media file
    /// </summary>
    [HttpGet("media/{mediaId:guid}/jobs")]
    [ProducesResponseType(typeof(List<MediaProcessingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MediaProcessingResponse>>> GetMediaProcessingJobsAsync(Guid mediaId)
    {
        try
        {
            var jobs = await _mediaProcessingService.GetMediaProcessingJobsAsync(mediaId);
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving processing jobs for media {MediaId}", mediaId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving processing jobs" });
        }
    }

    /// <summary>
    /// Process media file immediately (synchronous processing)
    /// </summary>
    [HttpPost("process-immediate")]
    [ProducesResponseType(typeof(MediaProcessingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    public async Task<ActionResult<MediaProcessingResponse>> ProcessImmediatelyAsync([FromBody] MediaProcessingRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _mediaProcessingService.ProcessImmediatelyAsync(request);
            
            _logger.LogInformation("Immediate processing completed: {JobId} for media {MediaId} (Status: {Status})", 
                response.JobId, response.MediaId, response.Status);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Immediate processing validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning("Immediate processing timed out for media {MediaId}: {Message}", request.MediaId, ex.Message);
            return StatusCode(StatusCodes.Status408RequestTimeout, 
                new { error = "Processing request timed out. Try submitting as an asynchronous job instead." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during immediate processing for media {MediaId}", request.MediaId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred during immediate processing" });
        }
    }

    /// <summary>
    /// Get processing queue status
    /// </summary>
    [HttpGet("queue/status")]
    [ProducesResponseType(typeof(ProcessingQueueStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProcessingQueueStatus>> GetQueueStatusAsync()
    {
        try
        {
            var status = await _mediaProcessingService.GetQueueStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving processing queue status");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving queue status" });
        }
    }

    /// <summary>
    /// Get processing job history
    /// </summary>
    [HttpGet("jobs")]
    [ProducesResponseType(typeof(ProcessingHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProcessingHistoryResponse>> GetProcessingHistoryAsync(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var jobs = await _mediaProcessingService.GetProcessingHistoryAsync(page, pageSize);
            
            var response = new ProcessingHistoryResponse
            {
                Jobs = jobs,
                Page = page,
                PageSize = pageSize,
                TotalCount = jobs.Count // Note: This should be improved to get actual total count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving processing job history");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving processing history" });
        }
    }

    /// <summary>
    /// Clean up old processing jobs (Admin only)
    /// </summary>
    [HttpPost("cleanup")]
    [ProducesResponseType(typeof(CleanupResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProcessingCleanupResult>> CleanupOldJobsAsync([FromBody] ProcessingCleanupRequest? request = null)
    {
        try
        {
            var maxAge = request?.MaxAgeDays != null 
                ? TimeSpan.FromDays(request.MaxAgeDays.Value)
                : TimeSpan.FromDays(7); // Default 7 days

            var deletedCount = await _mediaProcessingService.CleanupOldJobsAsync(maxAge);

            _logger.LogInformation("Processing jobs cleanup completed: {DeletedCount} jobs removed", deletedCount);

            return Ok(new ProcessingCleanupResult 
            { 
                DeletedJobsCount = deletedCount,
                MaxAge = maxAge,
                CompletedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during processing jobs cleanup");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred during cleanup" });
        }
    }

    /// <summary>
    /// Get processing statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ProcessingStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProcessingStats>> GetProcessingStatsAsync([FromQuery] int? days = null)
    {
        try
        {
            var queueStatus = await _mediaProcessingService.GetQueueStatusAsync();
            
            // Create stats from queue status - this could be enhanced with more detailed analytics
            var stats = new ProcessingStats
            {
                TotalJobs = queueStatus.TotalJobs,
                QueuedJobs = queueStatus.QueuedJobs,
                ProcessingJobs = queueStatus.ProcessingJobs,
                CompletedJobs = queueStatus.CompletedJobs,
                FailedJobs = queueStatus.FailedJobs,
                JobsByType = queueStatus.JobsByType,
                JobsByStatus = queueStatus.JobsByStatus,
                ActiveWorkers = queueStatus.Workers.Count(w => w.IsActive),
                LastProcessedAt = queueStatus.LastProcessedAt
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving processing statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving processing statistics" });
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

// Additional DTOs for controller-specific responses
public class ProcessingHistoryResponse
{
    public List<MediaProcessingResponse> Jobs { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public class ProcessingCleanupRequest
{
    public int? MaxAgeDays { get; set; }
}

public class ProcessingCleanupResult
{
    public int DeletedJobsCount { get; set; }
    public TimeSpan MaxAge { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class ProcessingStats
{
    public int TotalJobs { get; set; }
    public int QueuedJobs { get; set; }
    public int ProcessingJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
    public int ActiveWorkers { get; set; }
    public Dictionary<MediaProcessingType, int> JobsByType { get; set; } = new();
    public Dictionary<MediaProcessingStatus, int> JobsByStatus { get; set; } = new();
    public DateTime LastProcessedAt { get; set; }
}
