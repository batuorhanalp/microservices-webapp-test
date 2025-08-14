using System.ComponentModel.DataAnnotations;

namespace WebApp.Common.DTOs;

/// <summary>
/// Request to upload a single media file
/// </summary>
public class MediaUploadRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
    
    public Guid? PostId { get; set; }
    
    public string? AltText { get; set; }
    
    public string? Description { get; set; }
    
    /// <summary>
    /// Tags for the media content (comma-separated)
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// Whether this is a temporary upload (will be cleaned up if not associated with content)
    /// </summary>
    public bool IsTemporary { get; set; } = true;
}

/// <summary>
/// Response after successful media upload
/// </summary>
public class MediaUploadResponse
{
    public Guid Id { get; set; }
    
    public string Url { get; set; } = string.Empty;
    
    public string FileName { get; set; } = string.Empty;
    
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public string FileSizeFormatted { get; set; } = string.Empty;
    
    public int? Width { get; set; }
    
    public int? Height { get; set; }
    
    public int? Duration { get; set; }
    
    public string? ThumbnailUrl { get; set; }
    
    public bool IsImage { get; set; }
    
    public bool IsVideo { get; set; }
    
    public bool IsAudio { get; set; }
    
    public DateTime UploadedAt { get; set; }
    
    public MediaProcessingStatus ProcessingStatus { get; set; }
}

/// <summary>
/// Request to upload multiple media files
/// </summary>
public class BulkMediaUploadRequest
{
    [Required]
    public IFormFileCollection Files { get; set; } = null!;
    
    public Guid? PostId { get; set; }
    
    public bool IsTemporary { get; set; } = true;
    
    /// <summary>
    /// Individual file metadata (index-based)
    /// </summary>
    public Dictionary<int, MediaFileMetadata> FileMetadata { get; set; } = new();
}

/// <summary>
/// Metadata for individual file in bulk upload
/// </summary>
public class MediaFileMetadata
{
    public string? AltText { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Response for bulk upload
/// </summary>
public class BulkMediaUploadResponse
{
    public List<MediaUploadResponse> SuccessfulUploads { get; set; } = new();
    public List<MediaUploadError> FailedUploads { get; set; } = new();
    public int TotalFiles { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

/// <summary>
/// Error information for failed uploads
/// </summary>
public class MediaUploadError
{
    public string FileName { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int FileIndex { get; set; }
}

/// <summary>
/// Media processing request
/// </summary>
public class MediaProcessingRequest
{
    public Guid MediaId { get; set; }
    
    public MediaProcessingType ProcessingType { get; set; }
    
    /// <summary>
    /// Processing parameters (JSON object)
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public int Priority { get; set; } = 5; // 1 (highest) to 10 (lowest)
}

/// <summary>
/// Media processing response
/// </summary>
public class MediaProcessingResponse
{
    public Guid JobId { get; set; }
    
    public Guid MediaId { get; set; }
    
    public MediaProcessingType ProcessingType { get; set; }
    
    public MediaProcessingStatus Status { get; set; }
    
    public DateTime StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public int Progress { get; set; } // 0-100
    
    public string? ResultUrl { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Request to get media processing status
/// </summary>
public class ProcessingStatusRequest
{
    public Guid JobId { get; set; }
}

/// <summary>
/// Media search/filter request
/// </summary>
public class MediaSearchRequest
{
    public string? SearchTerm { get; set; }
    
    public List<string> ContentTypes { get; set; } = new();
    
    public long? MinFileSize { get; set; }
    
    public long? MaxFileSize { get; set; }
    
    public DateTime? UploadedAfter { get; set; }
    
    public DateTime? UploadedBefore { get; set; }
    
    public List<string> Tags { get; set; } = new();
    
    public Guid? PostId { get; set; }
    
    public Guid? UserId { get; set; }
    
    public int Page { get; set; } = 1;
    
    public int PageSize { get; set; } = 20;
    
    public string SortBy { get; set; } = "CreatedAt";
    
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Media search response
/// </summary>
public class MediaSearchResponse
{
    public List<MediaUploadResponse> Media { get; set; } = new();
    
    public int TotalCount { get; set; }
    
    public int Page { get; set; }
    
    public int PageSize { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasNextPage { get; set; }
    
    public bool HasPreviousPage { get; set; }
}

/// <summary>
/// Request to update media metadata
/// </summary>
public class UpdateMediaRequest
{
    public string? AltText { get; set; }
    
    public string? Description { get; set; }
    
    public string? Tags { get; set; }
    
    public bool? IsActive { get; set; }
}

/// <summary>
/// Media storage statistics
/// </summary>
public class MediaStorageStats
{
    public int TotalFiles { get; set; }
    
    public long TotalSizeBytes { get; set; }
    
    public string TotalSizeFormatted { get; set; } = string.Empty;
    
    public int ImageCount { get; set; }
    
    public int VideoCount { get; set; }
    
    public int AudioCount { get; set; }
    
    public int OtherCount { get; set; }
    
    public long ImageSizeBytes { get; set; }
    
    public long VideoSizeBytes { get; set; }
    
    public long AudioSizeBytes { get; set; }
    
    public long OtherSizeBytes { get; set; }
    
    public Dictionary<string, int> ContentTypeCounts { get; set; } = new();
    
    public Dictionary<string, long> ContentTypeSizes { get; set; } = new();
}

/// <summary>
/// Enums for media processing
/// </summary>
public enum MediaProcessingType
{
    /// <summary>
    /// Generate thumbnail for image/video
    /// </summary>
    ThumbnailGeneration = 1,
    
    /// <summary>
    /// Resize image to specific dimensions
    /// </summary>
    ImageResize = 2,
    
    /// <summary>
    /// Compress image/video
    /// </summary>
    Compression = 3,
    
    /// <summary>
    /// Convert format (e.g., PNG to JPEG)
    /// </summary>
    FormatConversion = 4,
    
    /// <summary>
    /// Extract metadata from media file
    /// </summary>
    MetadataExtraction = 5,
    
    /// <summary>
    /// Generate multiple sizes/variants
    /// </summary>
    VariantGeneration = 6,
    
    /// <summary>
    /// Video transcoding
    /// </summary>
    VideoTranscoding = 7,
    
    /// <summary>
    /// Audio transcoding
    /// </summary>
    AudioTranscoding = 8,
    
    /// <summary>
    /// Content analysis (AI/ML processing)
    /// </summary>
    ContentAnalysis = 9,
    
    /// <summary>
    /// OCR text extraction
    /// </summary>
    TextExtraction = 10
}

/// <summary>
/// Media processing status
/// </summary>
public enum MediaProcessingStatus
{
    /// <summary>
    /// Processing not started
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Currently being processed
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Successfully completed
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// Processing failed
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// Processing cancelled
    /// </summary>
    Cancelled = 4,
    
    /// <summary>
    /// Queued for processing
    /// </summary>
    Queued = 5
}
