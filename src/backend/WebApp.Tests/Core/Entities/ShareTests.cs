using FluentAssertions;
using WebApp.Core.Entities;
using Xunit;

namespace WebApp.Tests.Core.Entities;

public class ShareTests
{
    [Fact]
    public void CreateShare_WithValidDataAndComment_ShouldCreateShareWithCorrectProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var comment = "Great post!";

        // Act
        var share = new Share(userId, postId, comment);

        // Assert
        share.Id.Should().NotBeEmpty();
        share.UserId.Should().Be(userId);
        share.PostId.Should().Be(postId);
        share.Comment.Should().Be(comment);
        share.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateShare_WithoutComment_ShouldCreateShareWithNullComment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var share = new Share(userId, postId);

        // Assert
        share.Comment.Should().BeNull();
    }

    [Fact]
    public void CreateShare_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Share(Guid.Empty, Guid.NewGuid());

        action.Should().Throw<ArgumentException>()
            .WithMessage("User ID is required*")
            .And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void CreateShare_WithEmptyPostId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Share(Guid.NewGuid(), Guid.Empty);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Post ID is required*")
            .And.ParamName.Should().Be("postId");
    }

    [Fact]
    public void CreateShare_WithWhitespaceComment_ShouldTrimComment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var comment = "  Great post!  ";

        // Act
        var share = new Share(userId, postId, comment);

        // Assert
        share.Comment.Should().Be("Great post!");
    }

    [Fact]
    public void UpdateComment_WithValidComment_ShouldUpdateComment()
    {
        // Arrange
        var share = new Share(Guid.NewGuid(), Guid.NewGuid(), "Original comment");
        var newComment = "Updated comment";

        // Act
        share.UpdateComment(newComment);

        // Assert
        share.Comment.Should().Be(newComment);
    }

    [Fact]
    public void UpdateComment_WithNullComment_ShouldSetCommentToNull()
    {
        // Arrange
        var share = new Share(Guid.NewGuid(), Guid.NewGuid(), "Original comment");

        // Act
        share.UpdateComment(null);

        // Assert
        share.Comment.Should().BeNull();
    }

    [Fact]
    public void UpdateComment_WithWhitespaceComment_ShouldTrimComment()
    {
        // Arrange
        var share = new Share(Guid.NewGuid(), Guid.NewGuid());
        var comment = "  Updated comment  ";

        // Act
        share.UpdateComment(comment);

        // Assert
        share.Comment.Should().Be("Updated comment");
    }
}
