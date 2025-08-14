using FluentAssertions;
using WebApp.Common.Entities;
using Xunit;

namespace WebApp.Tests.Core.Entities;

public class CommentTests
{
    [Fact]
    public void CreateComment_WithValidData_ShouldCreateCommentWithCorrectProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var content = "This is a comment";

        // Act
        var comment = new Comment(userId, postId, content);

        // Assert
        comment.Id.Should().NotBeEmpty();
        comment.UserId.Should().Be(userId);
        comment.PostId.Should().Be(postId);
        comment.Content.Should().Be(content);
        comment.IsEdited.Should().BeFalse();
        comment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        comment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateComment_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Comment(Guid.Empty, Guid.NewGuid(), "Content");

        action.Should().Throw<ArgumentException>()
            .WithMessage("User ID is required*")
            .And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void CreateComment_WithEmptyPostId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Comment(Guid.NewGuid(), Guid.Empty, "Content");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Post ID is required*")
            .And.ParamName.Should().Be("postId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateComment_WithInvalidContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Act & Assert
        var action = () => new Comment(Guid.NewGuid(), Guid.NewGuid(), invalidContent);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Comment content is required*")
            .And.ParamName.Should().Be("content");
    }

    [Fact]
    public void CreateComment_WithWhitespaceContent_ShouldTrimContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var content = "  This is a comment with spaces  ";

        // Act
        var comment = new Comment(userId, postId, content);

        // Assert
        comment.Content.Should().Be("This is a comment with spaces");
    }

    [Fact]
    public void UpdateContent_WithValidContent_ShouldUpdateContentAndMarkAsEdited()
    {
        // Arrange
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "Original content");
        var newContent = "Updated content";
        var originalUpdatedAt = comment.UpdatedAt;
        Thread.Sleep(1);

        // Act
        comment.UpdateContent(newContent);

        // Assert
        comment.Content.Should().Be(newContent);
        comment.IsEdited.Should().BeTrue();
        comment.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateContent_WithInvalidContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Arrange
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "Original content");

        // Act & Assert
        var action = () => comment.UpdateContent(invalidContent);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Comment content cannot be empty*")
            .And.ParamName.Should().Be("content");
    }

    [Fact]
    public void UpdateContent_WithWhitespaceContent_ShouldTrimContent()
    {
        // Arrange
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "Original content");
        var newContent = "  Updated content with spaces  ";

        // Act
        comment.UpdateContent(newContent);

        // Assert
        comment.Content.Should().Be("Updated content with spaces");
        comment.IsEdited.Should().BeTrue();
    }
}
