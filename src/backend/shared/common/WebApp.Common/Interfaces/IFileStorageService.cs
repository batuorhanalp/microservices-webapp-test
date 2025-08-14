namespace WebApp.Common.Interfaces;

/// <summary>
/// Interface for file storage operations (local filesystem, cloud storage, etc.)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file and return its URL
    /// </summary>
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Upload a file to a specific path and return its URL
    /// </summary>
    Task<string> UploadAsync(Stream fileStream, string filePath, string fileName, string contentType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Download a file as stream
    /// </summary>
    Task<Stream> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a file
    /// </summary>
    Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if file exists
    /// </summary>
    Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get file information
    /// </summary>
    Task<FileStorageInfo?> GetFileInfoAsync(string fileUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate a pre-signed URL for temporary access
    /// </summary>
    Task<string> GeneratePresignedUrlAsync(string fileUrl, TimeSpan expiry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Copy file to a new location
    /// </summary>
    Task<string> CopyAsync(string sourceUrl, string destinationPath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Move file to a new location
    /// </summary>
    Task<string> MoveAsync(string sourceUrl, string destinationPath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// List files in a directory/path
    /// </summary>
    Task<List<FileStorageInfo>> ListFilesAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get storage usage statistics
    /// </summary>
    Task<StorageStats> GetStorageStatsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// File storage information
/// </summary>
public class FileStorageInfo
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Storage usage statistics
/// </summary>
public class StorageStats
{
    public long TotalBytes { get; set; }
    public long UsedBytes { get; set; }
    public long AvailableBytes { get; set; }
    public int FileCount { get; set; }
    public string TotalSizeFormatted { get; set; } = string.Empty;
    public string UsedSizeFormatted { get; set; } = string.Empty;
    public string AvailableSizeFormatted { get; set; } = string.Empty;
    public Dictionary<string, long> SizeByContentType { get; set; } = new();
    public Dictionary<string, int> CountByContentType { get; set; } = new();
}
