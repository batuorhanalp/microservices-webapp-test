using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
using WebApp.Common.Services;
using Xunit;

namespace WebApp.Tests.Application.Services;

// TODO: Implement LikeService in WebApp.Common.Services before uncommenting
/*
public class LikeServiceTests
{
    private readonly Mock<ILikeRepository> _mockLikeRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPostRepository> _mockPostRepository;
    private readonly Mock<ILogger<LikeService>> _mockLogger;
    private readonly LikeService _likeService;

    public LikeServiceTests()
    {
        _mockLikeRepository = new Mock<ILikeRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPostRepository = new Mock<IPostRepository>();
        _mockLogger = new Mock<ILogger<LikeService>>();
        _likeService = new LikeService(_mockLikeRepository.Object, _mockUserRepository.Object, _mockPostRepository.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLikeRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new LikeService(null!, _mockUserRepository.Object, _mockPostRepository.Object, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*likeRepository*");
    }

    [Fact]
    public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new LikeService(_mockLikeRepository.Object, null!, _mockPostRepository.Object, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*userRepository*");
    }

    [Fact]
    public void Constructor_WithNullPostRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new LikeService(_mockLikeRepository.Object, _mockUserRepository.Object, null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*postRepository*");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new LikeService(_mockLikeRepository.Object, _mockUserRepository.Object, _mockPostRepository.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*logger*");
    }

    #endregion

    #region LikePostAsync Tests

    [Fact]
    public async Task LikePostAsync_WithValidUserAndPost_ShouldCreateLike()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var user = new User("user@example.com", "username", "User Name", "hashedpassword");
        var post = new Post(userId, "Test content");
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(post);
        _mockLikeRepository.Setup(x => x.ExistsAsync(userId, postId))
            .ReturnsAsync(false);
        _mockLikeRepository.Setup(x => x.AddAsync(It.IsAny<Like>()))
            .Returns(Task.CompletedTask);
        _mockLikeRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _likeService.LikePostAsync(userId, postId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.PostId.Should().Be(postId);
        
        _mockLikeRepository.Verify(x => x.AddAsync(It.IsAny<Like>()), Times.Once);
        _mockLikeRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LikePostAsync_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.Empty;
        var postId = Guid.NewGuid();

        // Act & Assert
        var act = async () => await _likeService.LikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*User ID cannot be empty*");
    }

    [Fact]
    public async Task LikePostAsync_WithEmptyPostId_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.Empty;

        // Act & Assert
        var act = async () => await _likeService.LikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Post ID cannot be empty*");
    }

    [Fact]
    public async Task LikePostAsync_WithNonExistentUser_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var act = async () => await _likeService.LikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*User not found*");
    }

    [Fact]
    public async Task LikePostAsync_WithNonExistentPost_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var user = new User("user@example.com", "username", "User Name", "hashedpassword");
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        var act = async () => await _likeService.LikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Post not found*");
    }

    [Fact]
    public async Task LikePostAsync_WhenLikeAlreadyExists_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var user = new User("user@example.com", "username", "User Name", "hashedpassword");
        var post = new Post(userId, "Test content");
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(post);
        _mockLikeRepository.Setup(x => x.ExistsAsync(userId, postId))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await _likeService.LikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*User has already liked this post*");
    }

    #endregion

    #region UnlikePostAsync Tests

    [Fact]
    public async Task UnlikePostAsync_WithValidUserAndPost_ShouldRemoveLike()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var user = new User("user@example.com", "username", "User Name", "hashedpassword");
        var post = new Post(userId, "Test content");
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(post);
        _mockLikeRepository.Setup(x => x.ExistsAsync(userId, postId))
            .ReturnsAsync(true);
        _mockLikeRepository.Setup(x => x.DeleteByUserAndPostAsync(userId, postId))
            .Returns(Task.CompletedTask);
        _mockLikeRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _likeService.UnlikePostAsync(userId, postId);

        // Assert
        _mockLikeRepository.Verify(x => x.DeleteByUserAndPostAsync(userId, postId), Times.Once);
        _mockLikeRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UnlikePostAsync_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.Empty;
        var postId = Guid.NewGuid();

        // Act & Assert
        var act = async () => await _likeService.UnlikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*User ID cannot be empty*");
    }

    [Fact]
    public async Task UnlikePostAsync_WithEmptyPostId_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.Empty;

        // Act & Assert
        var act = async () => await _likeService.UnlikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Post ID cannot be empty*");
    }

    [Fact]
    public async Task UnlikePostAsync_WithNonExistentUser_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var act = async () => await _likeService.UnlikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*User not found*");
    }

    [Fact]
    public async Task UnlikePostAsync_WithNonExistentPost_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var user = new User("user@example.com", "username", "User Name", "hashedpassword");
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        var act = async () => await _likeService.UnlikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Post not found*");
    }

    [Fact]
    public async Task UnlikePostAsync_WhenLikeDoesNotExist_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var user = new User("user@example.com", "username", "User Name", "hashedpassword");
        var post = new Post(userId, "Test content");
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(post);
        _mockLikeRepository.Setup(x => x.ExistsAsync(userId, postId))
            .ReturnsAsync(false);

        // Act & Assert
        var act = async () => await _likeService.UnlikePostAsync(userId, postId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*User has not liked this post*");
    }

    #endregion

    #region HasUserLikedPostAsync Tests

    [Fact]
    public async Task HasUserLikedPostAsync_WhenLikeExists_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        
        _mockLikeRepository.Setup(x => x.ExistsAsync(userId, postId))
            .ReturnsAsync(true);

        // Act
        var result = await _likeService.HasUserLikedPostAsync(userId, postId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasUserLikedPostAsync_WhenLikeDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        
        _mockLikeRepository.Setup(x => x.ExistsAsync(userId, postId))
            .ReturnsAsync(false);

        // Act
        var result = await _likeService.HasUserLikedPostAsync(userId, postId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasUserLikedPostAsync_WithEmptyUserId_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.Empty;
        var postId = Guid.NewGuid();

        // Act
        var result = await _likeService.HasUserLikedPostAsync(userId, postId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasUserLikedPostAsync_WithEmptyPostId_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.Empty;

        // Act
        var result = await _likeService.HasUserLikedPostAsync(userId, postId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetPostLikesAsync Tests

    [Fact]
    public async Task GetPostLikesAsync_WithValidPostId_ShouldReturnLikes()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var likes = new List<Like>
        {
            new Like(Guid.NewGuid(), postId),
            new Like(Guid.NewGuid(), postId)
        };
        
        _mockLikeRepository.Setup(x => x.GetByPostAsync(postId, 50, 0))
            .ReturnsAsync(likes);

        // Act
        var result = await _likeService.GetPostLikesAsync(postId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(like => like.PostId.Should().Be(postId));
    }

    [Fact]
    public async Task GetPostLikesAsync_WithEmptyPostId_ShouldReturnEmpty()
    {
        // Arrange
        var postId = Guid.Empty;

        // Act
        var result = await _likeService.GetPostLikesAsync(postId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPostLikesAsync_WithInvalidLimit_ShouldUseDefaultLimit()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var likes = new List<Like>();
        
        _mockLikeRepository.Setup(x => x.GetByPostAsync(postId, 50, 0))
            .ReturnsAsync(likes);

        // Act - Test negative limit
        await _likeService.GetPostLikesAsync(postId, -1);

        // Assert
        _mockLikeRepository.Verify(x => x.GetByPostAsync(postId, 50, 0), Times.Once);

        // Act - Test zero limit
        await _likeService.GetPostLikesAsync(postId, 0);

        // Assert
        _mockLikeRepository.Verify(x => x.GetByPostAsync(postId, 50, 0), Times.Exactly(2));
    }

    [Fact]
    public async Task GetPostLikesAsync_WithNegativeOffset_ShouldUseZeroOffset()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var likes = new List<Like>();
        
        _mockLikeRepository.Setup(x => x.GetByPostAsync(postId, 50, 0))
            .ReturnsAsync(likes);

        // Act
        await _likeService.GetPostLikesAsync(postId, offset: -10);

        // Assert
        _mockLikeRepository.Verify(x => x.GetByPostAsync(postId, 50, 0), Times.Once);
    }

    #endregion

    #region GetUserLikesAsync Tests

    [Fact]
    public async Task GetUserLikesAsync_WithValidUserId_ShouldReturnUserLikes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var likes = new List<Like>
        {
            new Like(userId, Guid.NewGuid()),
            new Like(userId, Guid.NewGuid())
        };
        
        _mockLikeRepository.Setup(x => x.GetByUserAsync(userId, 50, 0))
            .ReturnsAsync(likes);

        // Act
        var result = await _likeService.GetUserLikesAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(like => like.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetUserLikesAsync_WithEmptyUserId_ShouldReturnEmpty()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var result = await _likeService.GetUserLikesAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetPostLikeCountAsync Tests

    [Fact]
    public async Task GetPostLikeCountAsync_WithValidPostId_ShouldReturnCount()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var expectedCount = 5;
        
        _mockLikeRepository.Setup(x => x.GetCountByPostAsync(postId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _likeService.GetPostLikeCountAsync(postId);

        // Assert
        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetPostLikeCountAsync_WithEmptyPostId_ShouldReturnZero()
    {
        // Arrange
        var postId = Guid.Empty;

        // Act
        var result = await _likeService.GetPostLikeCountAsync(postId);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region GetUserLikeCountAsync Tests

    [Fact]
    public async Task GetUserLikeCountAsync_WithValidUserId_ShouldReturnCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedCount = 3;
        
        _mockLikeRepository.Setup(x => x.GetCountByUserAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _likeService.GetUserLikeCountAsync(userId);

        // Assert
        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetUserLikeCountAsync_WithEmptyUserId_ShouldReturnZero()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var result = await _likeService.GetUserLikeCountAsync(userId);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region GetUsersWhoLikedPostAsync Tests

    [Fact]
    public async Task GetUsersWhoLikedPostAsync_WithValidPostId_ShouldReturnUsers()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var users = new List<User>
        {
            new User("user1@example.com", "user1", "User One", "hashedpassword1"),
            new User("user2@example.com", "user2", "User Two", "hashedpassword2")
        };
        
        _mockLikeRepository.Setup(x => x.GetUsersByPostAsync(postId, 50, 0))
            .ReturnsAsync(users);

        // Act
        var result = await _likeService.GetUsersWhoLikedPostAsync(postId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Username == "user1");
        result.Should().Contain(u => u.Username == "user2");
    }

    [Fact]
    public async Task GetUsersWhoLikedPostAsync_WithEmptyPostId_ShouldReturnEmpty()
    {
        // Arrange
        var postId = Guid.Empty;

        // Act
        var result = await _likeService.GetUsersWhoLikedPostAsync(postId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
*/
