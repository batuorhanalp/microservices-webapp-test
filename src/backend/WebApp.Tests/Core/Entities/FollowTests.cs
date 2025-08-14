using FluentAssertions;
using WebApp.Common.Entities;
using Xunit;

namespace WebApp.Tests.Core.Entities;

public class FollowTests
{
    [Fact]
    public void CreateFollow_WithValidData_ShouldCreateFollowWithCorrectProperties()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followeeId = Guid.NewGuid();

        // Act
        var follow = new Follow(followerId, followeeId);

        // Assert
        follow.Id.Should().NotBeEmpty();
        follow.FollowerId.Should().Be(followerId);
        follow.FolloweeId.Should().Be(followeeId);
        follow.IsAccepted.Should().BeTrue(); // Auto-accepted by default
        follow.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        follow.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        follow.IsPending.Should().BeFalse();
    }

    [Fact]
    public void CreateFollow_WithApprovalRequired_ShouldCreatePendingFollow()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followeeId = Guid.NewGuid();

        // Act
        var follow = new Follow(followerId, followeeId, requiresApproval: true);

        // Assert
        follow.IsAccepted.Should().BeFalse();
        follow.AcceptedAt.Should().BeNull();
        follow.IsPending.Should().BeTrue();
    }

    [Fact]
    public void CreateFollow_WithSameFollowerAndFollowee_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var action = () => new Follow(userId, userId);

        action.Should().Throw<ArgumentException>()
            .WithMessage("User cannot follow themselves");
    }

    [Fact]
    public void CreateFollow_WithEmptyFollowerId_ShouldThrowArgumentException()
    {
        // Arrange
        var followeeId = Guid.NewGuid();

        // Act & Assert
        var action = () => new Follow(Guid.Empty, followeeId);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Follower ID is required*")
            .And.ParamName.Should().Be("followerId");
    }

    [Fact]
    public void CreateFollow_WithEmptyFolloweeId_ShouldThrowArgumentException()
    {
        // Arrange
        var followerId = Guid.NewGuid();

        // Act & Assert
        var action = () => new Follow(followerId, Guid.Empty);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Followee ID is required*")
            .And.ParamName.Should().Be("followeeId");
    }

    [Fact]
    public void Accept_PendingFollow_ShouldAcceptAndSetTimestamp()
    {
        // Arrange
        var follow = new Follow(Guid.NewGuid(), Guid.NewGuid(), requiresApproval: true);

        // Act
        follow.Accept();

        // Assert
        follow.IsAccepted.Should().BeTrue();
        follow.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        follow.IsPending.Should().BeFalse();
    }

    [Fact]
    public void Accept_AlreadyAcceptedFollow_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var follow = new Follow(Guid.NewGuid(), Guid.NewGuid(), requiresApproval: false);

        // Act & Assert
        var action = () => follow.Accept();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Follow request is already accepted");
    }

    [Fact]
    public void Reject_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var follow = new Follow(Guid.NewGuid(), Guid.NewGuid(), requiresApproval: true);

        // Act & Assert
        var action = () => follow.Reject();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("To reject a follow request, delete the record");
    }
}
