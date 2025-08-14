using FluentAssertions;
using WebApp.Common.Entities;
using Xunit;

namespace WebApp.Tests.Core.Entities;

public class LikeTests
{
    [Fact]
    public void CreateLike_WithValidData_ShouldCreateLikeWithCorrectProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var like = new Like(userId, postId);

        // Assert
        like.Id.Should().NotBeEmpty();
        like.UserId.Should().Be(userId);
        like.PostId.Should().Be(postId);
        like.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateLike_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Like(Guid.Empty, Guid.NewGuid());

        action.Should().Throw<ArgumentException>()
            .WithMessage("User ID is required*")
            .And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void CreateLike_WithEmptyPostId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Like(Guid.NewGuid(), Guid.Empty);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Post ID is required*")
            .And.ParamName.Should().Be("postId");
    }
}
