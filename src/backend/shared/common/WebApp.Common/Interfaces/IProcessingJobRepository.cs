using WebApp.Common.DTOs;
using WebApp.Common.Entities;

namespace WebApp.Common.Interfaces;

/// <summary>
/// Repository interface for ProcessingJob entity operations
/// </summary>
public interface IProcessingJobRepository
{
    /// <summary>
    /// Get processing job by ID
    /// </summary>
    Task<ProcessingJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all processing jobs for a media file
    /// </summary>
    Task<List<ProcessingJob>> GetByMediaIdAsync(Guid mediaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get jobs by status
    /// </summary>
    Task<List<ProcessingJob>> GetByStatusAsync(MediaProcessingStatus status, int limit = 100, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get next pending job to process (highest priority first)
    /// </summary>
    Task<ProcessingJob?> GetNextPendingJobAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create new processing job
    /// </summary>
    Task<ProcessingJob> CreateAsync(ProcessingJob job, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update processing job
    /// </summary>
    Task<ProcessingJob> UpdateAsync(ProcessingJob job, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete processing job
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get processing job history with pagination
    /// </summary>
    Task<List<ProcessingJob>> GetHistoryAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get queue statistics
    /// </summary>
    Task<ProcessingQueueStats> GetQueueStatsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clean up old completed/failed jobs
    /// </summary>
    Task<int> CleanupOldJobsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get jobs that have been processing for too long (stuck jobs)
    /// </summary>
    Task<List<ProcessingJob>> GetStuckJobsAsync(TimeSpan maxProcessingTime, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get failed jobs that can be retried
    /// </summary>
    Task<List<ProcessingJob>> GetRetriableJobsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Processing queue statistics
/// </summary>
public class ProcessingQueueStats
{
    public int PendingJobs { get; set; }
    public int QueuedJobs { get; set; }
    public int ProcessingJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
    public int CancelledJobs { get; set; }
    public int TotalJobs { get; set; }
    public DateTime? LastProcessedAt { get; set; }
    public Dictionary<MediaProcessingType, int> JobsByType { get; set; } = new();
    public Dictionary<MediaProcessingStatus, int> JobsByStatus { get; set; } = new();
    public TimeSpan? AverageProcessingTime { get; set; }
}
