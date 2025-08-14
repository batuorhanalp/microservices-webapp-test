using WebApp.Common.DTOs;

namespace WebApp.Common.Interfaces;

/// <summary>
/// Interface for media processing operations
/// </summary>
public interface IMediaProcessingService
{
    /// <summary>
    /// Submit a media file for processing
    /// </summary>
    Task<MediaProcessingResponse> SubmitProcessingJobAsync(MediaProcessingRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get processing job status
    /// </summary>
    Task<MediaProcessingResponse?> GetProcessingStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancel a processing job
    /// </summary>
    Task<bool> CancelProcessingJobAsync(Guid jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all processing jobs for a media file
    /// </summary>
    Task<List<MediaProcessingResponse>> GetMediaProcessingJobsAsync(Guid mediaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process media file immediately (synchronous)
    /// </summary>
    Task<MediaProcessingResponse> ProcessImmediatelyAsync(MediaProcessingRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get processing queue status
    /// </summary>
    Task<ProcessingQueueStatus> GetQueueStatusAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retry failed processing job
    /// </summary>
    Task<MediaProcessingResponse> RetryProcessingJobAsync(Guid jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get processing job history
    /// </summary>
    Task<List<MediaProcessingResponse>> GetProcessingHistoryAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clean up completed/failed jobs older than specified time
    /// </summary>
    Task<int> CleanupOldJobsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
}

/// <summary>
/// Processing queue status information
/// </summary>
public class ProcessingQueueStatus
{
    public int QueuedJobs { get; set; }
    public int ProcessingJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
    public int TotalJobs { get; set; }
    public DateTime LastProcessedAt { get; set; }
    public Dictionary<MediaProcessingType, int> JobsByType { get; set; } = new();
    public Dictionary<MediaProcessingStatus, int> JobsByStatus { get; set; } = new();
    public List<WorkerStatus> Workers { get; set; } = new();
}

/// <summary>
/// Individual worker status
/// </summary>
public class WorkerStatus
{
    public string WorkerId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid? CurrentJobId { get; set; }
    public MediaProcessingType? CurrentJobType { get; set; }
    public DateTime? JobStartedAt { get; set; }
    public int Progress { get; set; }
    public DateTime LastHeartbeat { get; set; }
}
