using WebApp.Common.DTOs;
using WebApp.Common.Entities;

namespace WebApp.Common.Interfaces;

/// <summary>
/// Interface for media upload operations
/// </summary>
public interface IMediaUploadService
{
    /// <summary>
    /// Upload a single media file
    /// </summary>
    Task<MediaUploadResponse> UploadAsync(MediaUploadRequest request, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Upload multiple media files
    /// </summary>
    Task<BulkMediaUploadResponse> BulkUploadAsync(BulkMediaUploadRequest request, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get media file by ID
    /// </summary>
    Task<MediaUploadResponse?> GetByIdAsync(Guid mediaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search/filter media files
    /// </summary>
    Task<MediaSearchResponse> SearchAsync(MediaSearchRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update media metadata
    /// </summary>
    Task<MediaUploadResponse> UpdateAsync(Guid mediaId, UpdateMediaRequest request, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete media file
    /// </summary>
    Task<bool> DeleteAsync(Guid mediaId, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Associate temporary media with a post
    /// </summary>
    Task<bool> AssociateWithPostAsync(Guid mediaId, Guid postId, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get media storage statistics
    /// </summary>
    Task<MediaStorageStats> GetStorageStatsAsync(Guid? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clean up temporary/orphaned media files
    /// </summary>
    Task<int> CleanupTemporaryFilesAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate file upload constraints
    /// </summary>
    Task<ValidationResult> ValidateFileAsync(IFormFile file, CancellationToken cancellationToken = default);
}

/// <summary>
/// File validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}
