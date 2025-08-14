using FluentAssertions;
using WebApp.Core.Entities;
using Xunit;

namespace WebApp.Tests.Core.Entities;

public class MediaAttachmentTests
{
    [Fact]
    public void CreateMediaAttachment_WithValidData_ShouldCreateWithCorrectProperties()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var url = "https://example.com/image.jpg";
        var fileName = "image.jpg";
        var contentType = "image/jpeg";
        var fileSize = 1024L;

        // Act
        var attachment = new MediaAttachment(postId, url, fileName, contentType, fileSize);

        // Assert
        attachment.Id.Should().NotBeEmpty();
        attachment.PostId.Should().Be(postId);
        attachment.Url.Should().Be(url);
        attachment.FileName.Should().Be(fileName);
        attachment.ContentType.Should().Be(contentType);
        attachment.FileSize.Should().Be(fileSize);
        attachment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        attachment.AltText.Should().BeNull();
        attachment.Width.Should().BeNull();
        attachment.Height.Should().BeNull();
        attachment.Duration.Should().BeNull();
        attachment.ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public void CreateMediaAttachment_WithEmptyPostId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new MediaAttachment(Guid.Empty, "https://example.com/image.jpg", "image.jpg", "image/jpeg", 1024L);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Post ID is required*")
            .And.ParamName.Should().Be("postId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateMediaAttachment_WithInvalidUrl_ShouldThrowArgumentException(string invalidUrl)
    {
        // Act & Assert
        var action = () => new MediaAttachment(Guid.NewGuid(), invalidUrl, "image.jpg", "image/jpeg", 1024L);

        action.Should().Throw<ArgumentException>()
            .WithMessage("URL is required*")
            .And.ParamName.Should().Be("url");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateMediaAttachment_WithInvalidFileName_ShouldThrowArgumentException(string invalidFileName)
    {
        // Act & Assert
        var action = () => new MediaAttachment(Guid.NewGuid(), "https://example.com/image.jpg", invalidFileName, "image/jpeg", 1024L);

        action.Should().Throw<ArgumentException>()
            .WithMessage("File name is required*")
            .And.ParamName.Should().Be("fileName");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateMediaAttachment_WithInvalidContentType_ShouldThrowArgumentException(string invalidContentType)
    {
        // Act & Assert
        var action = () => new MediaAttachment(Guid.NewGuid(), "https://example.com/image.jpg", "image.jpg", invalidContentType, 1024L);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Content type is required*")
            .And.ParamName.Should().Be("contentType");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CreateMediaAttachment_WithInvalidFileSize_ShouldThrowArgumentException(long invalidFileSize)
    {
        // Act & Assert
        var action = () => new MediaAttachment(Guid.NewGuid(), "https://example.com/image.jpg", "image.jpg", "image/jpeg", invalidFileSize);

        action.Should().Throw<ArgumentException>()
            .WithMessage("File size must be positive*")
            .And.ParamName.Should().Be("fileSize");
    }

    [Fact]
    public void SetAltText_WithValidText_ShouldSetAltText()
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/image.jpg", "image.jpg", "image/jpeg", 1024L);
        var altText = "A beautiful sunset";

        // Act
        attachment.SetAltText(altText);

        // Assert
        attachment.AltText.Should().Be(altText);
    }

    [Fact]
    public void SetAltText_WithNullText_ShouldSetAltText()
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/image.jpg", "image.jpg", "image/jpeg", 1024L);

        // Act
        attachment.SetAltText(null);

        // Assert
        attachment.AltText.Should().BeNull();
    }

    [Fact]
    public void SetDimensions_WithValidDimensions_ShouldSetWidthAndHeight()
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/image.jpg", "image.jpg", "image/jpeg", 1024L);
        var width = 1920;
        var height = 1080;

        // Act
        attachment.SetDimensions(width, height);

        // Assert
        attachment.Width.Should().Be(width);
        attachment.Height.Should().Be(height);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(100, 0)]
    [InlineData(100, -1)]
    [InlineData(-1, -1)]
    public void SetDimensions_WithInvalidDimensions_ShouldThrowArgumentException(int width, int height)
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/image.jpg", "image.jpg", "image/jpeg", 1024L);

        // Act & Assert
        var action = () => attachment.SetDimensions(width, height);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Width and height must be positive values");
    }

    [Fact]
    public void SetDuration_WithValidDuration_ShouldSetDuration()
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/video.mp4", "video.mp4", "video/mp4", 2048L);
        var duration = 120; // 2 minutes

        // Act
        attachment.SetDuration(duration);

        // Assert
        attachment.Duration.Should().Be(duration);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void SetDuration_WithInvalidDuration_ShouldThrowArgumentException(int invalidDuration)
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/video.mp4", "video.mp4", "video/mp4", 2048L);

        // Act & Assert
        var action = () => attachment.SetDuration(invalidDuration);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Duration must be positive*")
            .And.ParamName.Should().Be("duration");
    }

    [Fact]
    public void SetThumbnailUrl_WithValidUrl_ShouldSetThumbnailUrl()
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/video.mp4", "video.mp4", "video/mp4", 2048L);
        var thumbnailUrl = "https://example.com/thumbnail.jpg";

        // Act
        attachment.SetThumbnailUrl(thumbnailUrl);

        // Assert
        attachment.ThumbnailUrl.Should().Be(thumbnailUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void SetThumbnailUrl_WithInvalidUrl_ShouldThrowArgumentException(string invalidUrl)
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/video.mp4", "video.mp4", "video/mp4", 2048L);

        // Act & Assert
        var action = () => attachment.SetThumbnailUrl(invalidUrl);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Thumbnail URL cannot be empty*")
            .And.ParamName.Should().Be("thumbnailUrl");
    }

    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("image/png", true)]
    [InlineData("image/gif", true)]
    [InlineData("video/mp4", false)]
    [InlineData("audio/mp3", false)]
    [InlineData("text/plain", false)]
    public void IsImage_ShouldReturnCorrectValue(string contentType, bool expectedResult)
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/file", "file", contentType, 1024L);

        // Act & Assert
        attachment.IsImage.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("video/mp4", true)]
    [InlineData("video/avi", true)]
    [InlineData("video/webm", true)]
    [InlineData("image/jpeg", false)]
    [InlineData("audio/mp3", false)]
    [InlineData("text/plain", false)]
    public void IsVideo_ShouldReturnCorrectValue(string contentType, bool expectedResult)
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/file", "file", contentType, 1024L);

        // Act & Assert
        attachment.IsVideo.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("audio/mp3", true)]
    [InlineData("audio/wav", true)]
    [InlineData("audio/ogg", true)]
    [InlineData("image/jpeg", false)]
    [InlineData("video/mp4", false)]
    [InlineData("text/plain", false)]
    public void IsAudio_ShouldReturnCorrectValue(string contentType, bool expectedResult)
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/file", "file", contentType, 1024L);

        // Act & Assert
        attachment.IsAudio.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(512, "512 B")]
    [InlineData(1024, "1.0 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1048576, "1.0 MB")]
    [InlineData(1073741824, "1.0 GB")]
    [InlineData(2147483648, "2.0 GB")]
    public void GetFileSizeFormatted_ShouldReturnCorrectFormattedSize(long fileSize, string expectedFormat)
    {
        // Arrange
        var attachment = new MediaAttachment(Guid.NewGuid(), "https://example.com/file", "file", "image/jpeg", fileSize);

        // Act
        var result = attachment.GetFileSizeFormatted();

        // Assert
        result.Should().Be(expectedFormat);
    }
}
