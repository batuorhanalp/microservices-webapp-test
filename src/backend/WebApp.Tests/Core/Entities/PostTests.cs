using FluentAssertions;
using WebApp.Common.Entities;
using Xunit;

namespace WebApp.Tests.Core.Entities;

public class PostTests
{
    [Fact]
    public void CreatePost_WithValidTextContent_ShouldCreatePostWithCorrectProperties()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var content = "This is a test post";

        // Act
        var post = new Post(authorId, content);

        // Assert
        post.Id.Should().NotBeEmpty();
        post.AuthorId.Should().Be(authorId);
        post.Content.Should().Be(content);
        post.Type.Should().Be(PostType.Text);
        post.Visibility.Should().Be(PostVisibility.Public);
        post.IsEdited.Should().BeFalse();
        post.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        post.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        post.ParentPostId.Should().BeNull();
        post.RootPostId.Should().BeNull();
        post.IsReply.Should().BeFalse();
    }

    [Fact]
    public void CreatePost_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Arrange
        var authorId = Guid.NewGuid();

        // Act & Assert
        var action = () => new Post(authorId, "", PostType.Text);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Content is required for text posts*")
            .And.ParamName.Should().Be("content");
    }

    [Fact]
    public void CreatePost_WithEmptyAuthorId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Post(Guid.Empty, "Test content");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Author ID is required*")
            .And.ParamName.Should().Be("authorId");
    }

    [Fact]
    public void CreateReplyPost_WithValidData_ShouldCreateReplyWithCorrectProperties()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var parentPostId = Guid.NewGuid();
        var content = "This is a reply";

        // Act
        var reply = new Post(authorId, content, parentPostId);

        // Assert
        reply.Id.Should().NotBeEmpty();
        reply.AuthorId.Should().Be(authorId);
        reply.Content.Should().Be(content);
        reply.ParentPostId.Should().Be(parentPostId);
        reply.RootPostId.Should().Be(parentPostId); // Should default to parent if no root specified
        reply.IsReply.Should().BeTrue();
    }

    [Fact]
    public void CreateReplyPost_WithRootSpecified_ShouldUseSpecifiedRoot()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var parentPostId = Guid.NewGuid();
        var rootPostId = Guid.NewGuid();
        var content = "This is a nested reply";

        // Act
        var reply = new Post(authorId, content, parentPostId, rootPostId);

        // Assert
        reply.ParentPostId.Should().Be(parentPostId);
        reply.RootPostId.Should().Be(rootPostId);
    }

    [Fact]
    public void UpdateContent_WithValidContent_ShouldUpdateContentAndMarkAsEdited()
    {
        // Arrange
        var post = new Post(Guid.NewGuid(), "Original content");
        var newContent = "Updated content";
        var originalUpdatedAt = post.UpdatedAt;
        Thread.Sleep(1);

        // Act
        post.UpdateContent(newContent);

        // Assert
        post.Content.Should().Be(newContent);
        post.IsEdited.Should().BeTrue();
        post.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateContent_WithEmptyContentForTextPost_ShouldThrowArgumentException()
    {
        // Arrange
        var post = new Post(Guid.NewGuid(), "Original content");

        // Act & Assert
        var action = () => post.UpdateContent("");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be empty for text posts*")
            .And.ParamName.Should().Be("content");
    }

    [Theory]
    [InlineData(PostVisibility.Public)]
    [InlineData(PostVisibility.Followers)]
    [InlineData(PostVisibility.Private)]
    public void SetVisibility_WithValidVisibility_ShouldUpdateVisibility(PostVisibility visibility)
    {
        // Arrange
        var post = new Post(Guid.NewGuid(), "Test content");
        var originalUpdatedAt = post.UpdatedAt;
        Thread.Sleep(1);

        // Act
        post.SetVisibility(visibility);

        // Assert
        post.Visibility.Should().Be(visibility);
        post.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void AddMediaAttachment_WithValidData_ShouldAddAttachmentAndUpdateType()
    {
        // Arrange
        var post = new Post(Guid.NewGuid(), "Post with image");
        var url = "https://example.com/image.jpg";
        var fileName = "image.jpg";
        var contentType = "image/jpeg";
        var fileSize = 1024L;
        var originalUpdatedAt = post.UpdatedAt;
        Thread.Sleep(1);

        // Act
        post.AddMediaAttachment(url, fileName, contentType, fileSize);

        // Assert
        post.MediaAttachments.Should().HaveCount(1);
        var attachment = post.MediaAttachments.First();
        attachment.Url.Should().Be(url);
        attachment.FileName.Should().Be(fileName);
        attachment.ContentType.Should().Be(contentType);
        attachment.FileSize.Should().Be(fileSize);
        post.Type.Should().Be(PostType.Image);
        post.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void CanBeViewedBy_PublicPost_ShouldReturnTrueForAnyUser()
    {
        // Arrange
        var author = new User("author@example.com", "author", "Author", "hashedpassword1");
        var viewer = new User("viewer@example.com", "viewer", "Viewer", "hashedpassword2");
        var post = new Post(author.Id, "Public post", PostType.Text, PostVisibility.Public);

        // Act & Assert
        post.CanBeViewedBy(viewer, author).Should().BeTrue();
    }

    [Fact]
    public void CanBeViewedBy_PrivatePost_ShouldReturnFalseForNonAuthor()
    {
        // Arrange
        var author = new User("author@example.com", "author", "Author", "hashedpassword1");
        var viewer = new User("viewer@example.com", "viewer", "Viewer", "hashedpassword2");
        var post = new Post(author.Id, "Private post", PostType.Text, PostVisibility.Private);

        // Act & Assert
        post.CanBeViewedBy(viewer, author).Should().BeFalse();
    }

    [Fact]
    public void CanBeViewedBy_PostByAuthor_ShouldReturnTrueForAuthor()
    {
        // Arrange
        var author = new User("author@example.com", "author", "Author", "hashedpassword");
        var post = new Post(author.Id, "Author's post", PostType.Text, PostVisibility.Private);

        // Act & Assert
        post.CanBeViewedBy(author, author).Should().BeTrue();
    }

    [Fact]
    public void CanBeViewedBy_FollowersOnlyPost_ShouldReturnTrueForAcceptedFollower()
    {
        // Arrange
        var author = new User("author@example.com", "author", "Author", "hashedpassword1");
        var follower = new User("follower@example.com", "follower", "Follower", "hashedpassword2");
        var post = new Post(author.Id, "Followers only post", PostType.Text, PostVisibility.Followers);
        
        // Add follower relationship
        var follow = new Follow(follower.Id, author.Id, requiresApproval: false);
        author.Followers.Add(follow);

        // Act & Assert
        post.CanBeViewedBy(follower, author).Should().BeTrue();
    }

    [Fact]
    public void CreatePost_WithMultipleMediaTypes_ShouldSetTypeToMixed()
    {
        // Arrange
        var post = new Post(Guid.NewGuid(), "Post with mixed media");

        // Act
        post.AddMediaAttachment("https://example.com/image.jpg", "image.jpg", "image/jpeg", 1024L);
        post.AddMediaAttachment("https://example.com/video.mp4", "video.mp4", "video/mp4", 2048L);

        // Assert
        post.Type.Should().Be(PostType.Mixed);
    }

    [Theory]
    [InlineData("image/jpeg", PostType.Image)]
    [InlineData("image/png", PostType.Image)]
    [InlineData("video/mp4", PostType.Video)]
    [InlineData("video/avi", PostType.Video)]
    [InlineData("audio/mp3", PostType.Audio)]
    [InlineData("audio/wav", PostType.Audio)]
    [InlineData("application/pdf", PostType.Text)]
    [InlineData("text/plain", PostType.Text)]
    public void AddMediaAttachment_WithDifferentContentTypes_ShouldSetCorrectPostType(string contentType, PostType expectedType)
    {
        // Arrange
        var post = new Post(Guid.NewGuid(), "Post with media");

        // Act
        post.AddMediaAttachment("https://example.com/file", "file", contentType, 1024L);

        // Assert
        post.Type.Should().Be(expectedType);
    }

    [Fact]
    public void CreatePost_WithNonTextTypeAndEmptyContent_ShouldNotThrowException()
    {
        // Arrange
        var authorId = Guid.NewGuid();

        // Act
        var post = new Post(authorId, "", PostType.Image, PostVisibility.Public);

        // Assert
        post.Content.Should().Be("");
        post.Type.Should().Be(PostType.Image);
    }

    [Fact]
    public void UpdateContent_WithNullContentForNonTextPost_ShouldSetToEmptyString()
    {
        // Arrange
        var post = new Post(Guid.NewGuid(), "Original content", PostType.Image);

        // Act
        post.UpdateContent(null);

        // Assert
        post.Content.Should().Be("");
    }

    [Fact]
    public void UpdateContent_WithNullContentForTextPost_ShouldThrowArgumentException()
    {
        // Arrange
        var post = new Post(Guid.NewGuid(), "Original content", PostType.Text);

        // Act & Assert
        var action = () => post.UpdateContent(null);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be empty for text posts*")
            .And.ParamName.Should().Be("content");
    }

    [Fact]
    public void CreatePost_WithNullContent_ShouldSetToEmptyString()
    {
        // Arrange
        var authorId = Guid.NewGuid();

        // Act
        var post = new Post(authorId, null, PostType.Image, PostVisibility.Public);

        // Assert
        post.Content.Should().Be("");
    }

    [Fact]
    public void AddMediaAttachment_ToPostWithoutMedia_ShouldChangeFromTextToMediaType()
    {
        // Arrange
        var post = new Post(Guid.NewGuid(), "Text post");
        post.Type.Should().Be(PostType.Text);

        // Act
        post.AddMediaAttachment("https://example.com/image.jpg", "image.jpg", "image/jpeg", 1024L);

        // Assert
        post.Type.Should().Be(PostType.Image);
    }

    [Fact]
    public void CanBeViewedBy_FollowersOnlyPost_ShouldReturnFalseForNonFollower()
    {
        // Arrange
        var author = new User("author@example.com", "author", "Author", "hashedpassword1");
        var nonFollower = new User("nonfollower@example.com", "nonfollower", "NonFollower", "hashedpassword2");
        var post = new Post(author.Id, "Followers only post", PostType.Text, PostVisibility.Followers);

        // Act & Assert
        post.CanBeViewedBy(nonFollower, author).Should().BeFalse();
    }

    [Fact]
    public void CanBeViewedBy_FollowersOnlyPost_ShouldReturnFalseForPendingFollower()
    {
        // Arrange
        var author = new User("author@example.com", "author", "Author", "hashedpassword1");
        var pendingFollower = new User("pending@example.com", "pending", "Pending", "hashedpassword2");
        var post = new Post(author.Id, "Followers only post", PostType.Text, PostVisibility.Followers);
        
        // Add pending follow relationship
        var pendingFollow = new Follow(pendingFollower.Id, author.Id, requiresApproval: true);
        // Don't accept the follow request
        author.Followers.Add(pendingFollow);

        // Act & Assert
        post.CanBeViewedBy(pendingFollower, author).Should().BeFalse();
    }
}
