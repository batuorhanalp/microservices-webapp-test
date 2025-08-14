using System.ComponentModel.DataAnnotations;
using WebApp.Common.DTOs;

namespace WebApp.Common.Entities;

public class ProcessingJob
{
    public Guid Id { get; private set; }
    
    [Required]
    public Guid MediaId { get; private set; }
    
    public MediaProcessingType ProcessingType { get; private set; }
    
    public MediaProcessingStatus Status { get; private set; }
    
    public int Priority { get; private set; }
    
    /// <summary>
    /// Processing parameters as JSON
    /// </summary>
    public string Parameters { get; private set; } = string.Empty;
    
    /// <summary>
    /// Processing result metadata as JSON
    /// </summary>
    public string? ResultMetadata { get; private set; }
    
    public string? ResultUrl { get; private set; }
    
    public string? ErrorMessage { get; private set; }
    
    public int Progress { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? StartedAt { get; private set; }
    
    public DateTime? CompletedAt { get; private set; }
    
    public string? WorkerId { get; private set; }
    
    public int RetryCount { get; private set; }
    
    public int MaxRetries { get; private set; } = 3;
    
    // Navigation properties
    public MediaAttachment Media { get; private set; } = null!;

    // Constructors
    private ProcessingJob() { } // For EF Core
    
    public ProcessingJob(
        Guid mediaId,
        MediaProcessingType processingType,
        string parameters,
        int priority = 5,
        int maxRetries = 3)
    {
        if (mediaId == Guid.Empty)
            throw new ArgumentException("Media ID is required", nameof(mediaId));
            
        if (string.IsNullOrWhiteSpace(parameters))
            throw new ArgumentException("Parameters are required", nameof(parameters));
            
        if (priority < 1 || priority > 10)
            throw new ArgumentException("Priority must be between 1 and 10", nameof(priority));

        Id = Guid.NewGuid();
        MediaId = mediaId;
        ProcessingType = processingType;
        Status = MediaProcessingStatus.Pending;
        Priority = priority;
        Parameters = parameters;
        MaxRetries = maxRetries;
        RetryCount = 0;
        Progress = 0;
        CreatedAt = DateTime.UtcNow;
    }

    // Domain methods
    public void StartProcessing(string workerId)
    {
        if (Status != MediaProcessingStatus.Pending && Status != MediaProcessingStatus.Queued)
            throw new InvalidOperationException($"Cannot start processing job in status {Status}");
            
        Status = MediaProcessingStatus.Processing;
        StartedAt = DateTime.UtcNow;
        WorkerId = workerId;
        Progress = 0;
    }
    
    public void UpdateProgress(int progress)
    {
        if (Status != MediaProcessingStatus.Processing)
            throw new InvalidOperationException("Can only update progress for processing jobs");
            
        if (progress < 0 || progress > 100)
            throw new ArgumentException("Progress must be between 0 and 100", nameof(progress));
            
        Progress = progress;
    }
    
    public void CompleteSuccessfully(string? resultUrl = null, string? resultMetadata = null)
    {
        if (Status != MediaProcessingStatus.Processing)
            throw new InvalidOperationException("Can only complete processing jobs");
            
        Status = MediaProcessingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Progress = 100;
        ResultUrl = resultUrl;
        ResultMetadata = resultMetadata;
        ErrorMessage = null;
    }
    
    public void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message is required", nameof(errorMessage));
            
        Status = MediaProcessingStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
    
    public void Cancel()
    {
        if (Status == MediaProcessingStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed job");
            
        Status = MediaProcessingStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void QueueForProcessing()
    {
        if (Status != MediaProcessingStatus.Pending)
            throw new InvalidOperationException("Can only queue pending jobs");
            
        Status = MediaProcessingStatus.Queued;
    }
    
    public void Retry()
    {
        if (Status != MediaProcessingStatus.Failed)
            throw new InvalidOperationException("Can only retry failed jobs");
            
        if (RetryCount >= MaxRetries)
            throw new InvalidOperationException($"Maximum retry attempts ({MaxRetries}) exceeded");
            
        RetryCount++;
        Status = MediaProcessingStatus.Pending;
        StartedAt = null;
        CompletedAt = null;
        Progress = 0;
        ErrorMessage = null;
        WorkerId = null;
    }
    
    public bool CanRetry => Status == MediaProcessingStatus.Failed && RetryCount < MaxRetries;
    
    public bool IsCompleted => Status == MediaProcessingStatus.Completed || 
                              Status == MediaProcessingStatus.Failed || 
                              Status == MediaProcessingStatus.Cancelled;
    
    public TimeSpan? ProcessingDuration
    {
        get
        {
            if (StartedAt == null) return null;
            var endTime = CompletedAt ?? DateTime.UtcNow;
            return endTime - StartedAt.Value;
        }
    }
}
