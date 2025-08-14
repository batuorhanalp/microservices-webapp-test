using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Application.Services;
using WebApp.Core.Entities;
using WebApp.Core.Interfaces;
using Xunit;

namespace WebApp.Tests.Application.Services;

public class PostServiceTests
{
    private readonly Mock<IPostRepository> _mockPostRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<PostService>> _mockLogger;
    private readonly IPostService _postService;

    public PostServiceTests()
    {
        _mockPostRepository = new Mock<IPostRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<PostService>>();
        _postService = new PostService(_mockPostRepository.Object, _mockUserRepository.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PostService(null!, _mockUserRepository.Object, _mockLogger.Object));
        
        exception.ParamName.Should().Be("postRepository");
    }

    [Fact]
    public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PostService(_mockPostRepository.Object, null!, _mockLogger.Object));
        
        exception.ParamName.Should().Be("userRepository");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PostService(_mockPostRepository.Object, _mockUserRepository.Object, null!));
        
        exception.ParamName.Should().Be("logger");
    }

    #endregion

    #region CreateTextPostAsync Tests

    [Fact]
    public async Task CreateTextPostAsync_WithValidData_ShouldCreatePost()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var content = "This is a test post content";
        var visibility = PostVisibility.Public;
        var author = new User("test@example.com", "testuser", "Test User");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _mockPostRepository.Setup(r => r.AddAsync(It.IsAny<Post>()))
            .Returns(Task.CompletedTask);
        _mockPostRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _postService.CreateTextPostAsync(authorId, content, visibility);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(content);
        result.AuthorId.Should().Be(authorId);
        result.Visibility.Should().Be(visibility);
        result.Type.Should().Be(PostType.Text);
        
        _mockPostRepository.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
        _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateTextPostAsync_WithEmptyAuthorId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateTextPostAsync(Guid.Empty, "content", PostVisibility.Public));
        
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task CreateTextPostAsync_WithInvalidContent_ShouldThrowException(string content)
    {
        // Arrange
        var authorId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateTextPostAsync(authorId, content, PostVisibility.Public));
        
        exception.Message.Should().Contain("Content is required for text posts");
    }

    [Fact]
    public async Task CreateTextPostAsync_WithNonExistentAuthor_ShouldThrowException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var content = "Test content";
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateTextPostAsync(authorId, content, PostVisibility.Public));
        
        exception.Message.Should().Contain("Author not found");
    }

    #endregion

    #region CreateMediaPostAsync Tests

    [Fact]
    public async Task CreateMediaPostAsync_WithValidData_ShouldCreatePost()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var content = "Optional content";
        var visibility = PostVisibility.Public;
        var author = new User("test@example.com", "testuser", "Test User");
        var mediaAttachments = new List<MediaAttachment>
        {
            new MediaAttachment(Guid.NewGuid(), "test.jpg", "https://example.com/test.jpg", "image/jpeg", 1024)
        };
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _mockPostRepository.Setup(r => r.AddAsync(It.IsAny<Post>()))
            .Returns(Task.CompletedTask);
        _mockPostRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _postService.CreateMediaPostAsync(authorId, content, mediaAttachments, visibility);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(content);
        result.AuthorId.Should().Be(authorId);
        result.Visibility.Should().Be(visibility);
        result.Type.Should().Be(PostType.Image);
        
        _mockPostRepository.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
        _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateMediaPostAsync_WithEmptyAuthorId_ShouldThrowException()
    {
        // Arrange
        var mediaAttachments = new List<MediaAttachment>
        {
            new MediaAttachment(Guid.NewGuid(), "test.jpg", "https://example.com/test.jpg", "image/jpeg", 1024)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateMediaPostAsync(Guid.Empty, "content", mediaAttachments, PostVisibility.Public));
        
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    [Fact]
    public async Task CreateMediaPostAsync_WithEmptyMediaAttachments_ShouldThrowException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var mediaAttachments = new List<MediaAttachment>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateMediaPostAsync(authorId, "content", mediaAttachments, PostVisibility.Public));
        
        exception.Message.Should().Contain("At least one media attachment is required");
    }

    [Fact]
    public async Task CreateMediaPostAsync_WithNullMediaAttachments_ShouldThrowException()
    {
        // Arrange
        var authorId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateMediaPostAsync(authorId, "content", null!, PostVisibility.Public));
        
        exception.Message.Should().Contain("Media attachments cannot be null");
    }

    #endregion

    #region CreateReplyAsync Tests

    [Fact]
    public async Task CreateReplyAsync_WithValidData_ShouldCreateReply()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var parentPostId = Guid.NewGuid();
        var content = "This is a reply";
        var visibility = PostVisibility.Public;
        var author = new User("test@example.com", "testuser", "Test User");
        var parentPost = new Post(Guid.NewGuid(), "Original post");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _mockPostRepository.Setup(r => r.GetByIdAsync(parentPostId))
            .ReturnsAsync(parentPost);
        _mockPostRepository.Setup(r => r.AddAsync(It.IsAny<Post>()))
            .Returns(Task.CompletedTask);
        _mockPostRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _postService.CreateReplyAsync(authorId, parentPostId, content, visibility);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(content);
        result.AuthorId.Should().Be(authorId);
        result.ParentPostId.Should().Be(parentPostId);
        result.Visibility.Should().Be(visibility);
        
        _mockPostRepository.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
        _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateReplyAsync_WithEmptyAuthorId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateReplyAsync(Guid.Empty, Guid.NewGuid(), "content", PostVisibility.Public));
        
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    [Fact]
    public async Task CreateReplyAsync_WithEmptyParentPostId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateReplyAsync(Guid.NewGuid(), Guid.Empty, "content", PostVisibility.Public));
        
        exception.Message.Should().Contain("Parent post ID cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task CreateReplyAsync_WithInvalidContent_ShouldThrowException(string content)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateReplyAsync(Guid.NewGuid(), Guid.NewGuid(), content, PostVisibility.Public));
        
        exception.Message.Should().Contain("Content is required for replies");
    }

    [Fact]
    public async Task CreateReplyAsync_WithNonExistentParentPost_ShouldThrowException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var parentPostId = Guid.NewGuid();
        var content = "Reply content";
        var author = new User("test@example.com", "testuser", "Test User");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _mockPostRepository.Setup(r => r.GetByIdAsync(parentPostId))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.CreateReplyAsync(authorId, parentPostId, content, PostVisibility.Public));
        
        exception.Message.Should().Contain("Parent post not found");
    }

    #endregion

    #region UpdatePostContentAsync Tests

    [Fact]
    public async Task UpdatePostContentAsync_WithValidData_ShouldUpdatePost()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var newContent = "Updated content";
        var post = new Post(authorId, "Original content");
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);
        _mockPostRepository.Setup(r => r.Update(It.IsAny<Post>()));
        _mockPostRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _postService.UpdatePostContentAsync(postId, authorId, newContent);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(newContent);
        
        _mockPostRepository.Verify(r => r.Update(It.IsAny<Post>()), Times.Once);
        _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdatePostContentAsync_WithEmptyPostId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.UpdatePostContentAsync(Guid.Empty, Guid.NewGuid(), "content"));
        
        exception.Message.Should().Contain("Post ID cannot be empty");
    }

    [Fact]
    public async Task UpdatePostContentAsync_WithEmptyAuthorId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.UpdatePostContentAsync(Guid.NewGuid(), Guid.Empty, "content"));
        
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task UpdatePostContentAsync_WithInvalidContent_ShouldThrowException(string newContent)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.UpdatePostContentAsync(Guid.NewGuid(), Guid.NewGuid(), newContent));
        
        exception.Message.Should().Contain("Content cannot be empty");
    }

    [Fact]
    public async Task UpdatePostContentAsync_WithNonExistentPost_ShouldThrowException()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var newContent = "Updated content";
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.UpdatePostContentAsync(postId, authorId, newContent));
        
        exception.Message.Should().Contain("Post not found");
    }

    [Fact]
    public async Task UpdatePostContentAsync_WithUnauthorizedUser_ShouldThrowException()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var actualAuthorId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();
        var newContent = "Updated content";
        var post = new Post(actualAuthorId, "Original content");
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.UpdatePostContentAsync(postId, unauthorizedUserId, newContent));
        
        exception.Message.Should().Contain("User is not authorized to update this post");
    }

    #endregion

    #region UpdatePostVisibilityAsync Tests

    [Fact]
    public async Task UpdatePostVisibilityAsync_WithValidData_ShouldUpdateVisibility()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var newVisibility = PostVisibility.Private;
        var post = new Post(authorId, "Content");
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);
        _mockPostRepository.Setup(r => r.Update(It.IsAny<Post>()));
        _mockPostRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _postService.UpdatePostVisibilityAsync(postId, authorId, newVisibility);

        // Assert
        result.Should().NotBeNull();
        result.Visibility.Should().Be(newVisibility);
        
        _mockPostRepository.Verify(r => r.Update(It.IsAny<Post>()), Times.Once);
        _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdatePostVisibilityAsync_WithEmptyPostId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.UpdatePostVisibilityAsync(Guid.Empty, Guid.NewGuid(), PostVisibility.Public));
        
        exception.Message.Should().Contain("Post ID cannot be empty");
    }

    [Fact]
    public async Task UpdatePostVisibilityAsync_WithEmptyAuthorId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.UpdatePostVisibilityAsync(Guid.NewGuid(), Guid.Empty, PostVisibility.Public));
        
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    #endregion

    #region DeletePostAsync Tests

    [Fact]
    public async Task DeletePostAsync_WithValidData_ShouldDeletePost()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var post = new Post(authorId, "Content");
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);
        _mockPostRepository.Setup(r => r.DeleteAsync(postId))
            .Returns(Task.CompletedTask);
        _mockPostRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _postService.DeletePostAsync(postId, authorId);

        // Assert
        result.Should().BeTrue();
        
        _mockPostRepository.Verify(r => r.DeleteAsync(postId), Times.Once);
        _mockPostRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePostAsync_WithEmptyPostId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.DeletePostAsync(Guid.Empty, Guid.NewGuid()));
        
        exception.Message.Should().Contain("Post ID cannot be empty");
    }

    [Fact]
    public async Task DeletePostAsync_WithEmptyAuthorId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.DeletePostAsync(Guid.NewGuid(), Guid.Empty));
        
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    [Fact]
    public async Task DeletePostAsync_WithNonExistentPost_ShouldThrowException()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.DeletePostAsync(postId, authorId));
        
        exception.Message.Should().Contain("Post not found");
    }

    [Fact]
    public async Task DeletePostAsync_WithUnauthorizedUser_ShouldThrowException()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var actualAuthorId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();
        var post = new Post(actualAuthorId, "Content");
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _postService.DeletePostAsync(postId, unauthorizedUserId));
        
        exception.Message.Should().Contain("User is not authorized to delete this post");
    }

    #endregion

    #region GetPostByIdAsync Tests

    [Fact]
    public async Task GetPostByIdAsync_WithValidIdAndViewerAccess_ShouldReturnPost()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var post = new Post(Guid.NewGuid(), "Content");
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);

        // Act
        var result = await _postService.GetPostByIdAsync(postId, viewerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(post);
        
        _mockPostRepository.Verify(r => r.GetByIdAsync(postId), Times.Once);
    }

    [Fact]
    public async Task GetPostByIdAsync_WithEmptyPostId_ShouldReturnNull()
    {
        // Act
        var result = await _postService.GetPostByIdAsync(Guid.Empty, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        _mockPostRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetPostByIdAsync_WithNonExistentPost_ShouldReturnNull()
    {
        // Arrange
        var postId = Guid.NewGuid();
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _postService.GetPostByIdAsync(postId, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPostsByAuthorAsync Tests

    [Fact]
    public async Task GetPostsByAuthorAsync_WithValidAuthorId_ShouldReturnPosts()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var limit = 10;
        var offset = 0;
        var expectedPosts = new List<Post>
        {
            new Post(authorId, "Post 1"),
            new Post(authorId, "Post 2")
        };
        
        _mockPostRepository.Setup(r => r.GetByAuthorAsync(authorId, limit, offset))
            .ReturnsAsync(expectedPosts);

        // Act
        var result = await _postService.GetPostsByAuthorAsync(authorId, viewerId, limit, offset);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedPosts);
        
        _mockPostRepository.Verify(r => r.GetByAuthorAsync(authorId, limit, offset), Times.Once);
    }

    [Fact]
    public async Task GetPostsByAuthorAsync_WithEmptyAuthorId_ShouldReturnEmpty()
    {
        // Act
        var result = await _postService.GetPostsByAuthorAsync(Guid.Empty, Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
        _mockPostRepository.Verify(r => r.GetByAuthorAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task GetPostsByAuthorAsync_WithInvalidLimit_ShouldUseDefaultLimit(int limit)
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var expectedPosts = new List<Post> { new Post(authorId, "Post") };
        
        _mockPostRepository.Setup(r => r.GetByAuthorAsync(authorId, 20, 0))
            .ReturnsAsync(expectedPosts);

        // Act
        var result = await _postService.GetPostsByAuthorAsync(authorId, null, limit);

        // Assert
        result.Should().BeEquivalentTo(expectedPosts);
        _mockPostRepository.Verify(r => r.GetByAuthorAsync(authorId, 20, 0), Times.Once);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetPostsByAuthorAsync_WithNegativeOffset_ShouldUseZeroOffset(int offset)
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var expectedPosts = new List<Post> { new Post(authorId, "Post") };
        
        _mockPostRepository.Setup(r => r.GetByAuthorAsync(authorId, 20, 0))
            .ReturnsAsync(expectedPosts);

        // Act
        var result = await _postService.GetPostsByAuthorAsync(authorId, null, 20, offset);

        // Assert
        result.Should().BeEquivalentTo(expectedPosts);
        _mockPostRepository.Verify(r => r.GetByAuthorAsync(authorId, 20, 0), Times.Once);
    }

    #endregion

    #region GetUserFeedAsync Tests

    [Fact]
    public async Task GetUserFeedAsync_WithValidUserId_ShouldReturnFeed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var limit = 10;
        var offset = 0;
        var expectedPosts = new List<Post>
        {
            new Post(Guid.NewGuid(), "Feed Post 1"),
            new Post(Guid.NewGuid(), "Feed Post 2")
        };
        
        _mockPostRepository.Setup(r => r.GetFeedAsync(userId, limit, offset))
            .ReturnsAsync(expectedPosts);

        // Act
        var result = await _postService.GetUserFeedAsync(userId, limit, offset);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedPosts);
        
        _mockPostRepository.Verify(r => r.GetFeedAsync(userId, limit, offset), Times.Once);
    }

    [Fact]
    public async Task GetUserFeedAsync_WithEmptyUserId_ShouldReturnEmpty()
    {
        // Act
        var result = await _postService.GetUserFeedAsync(Guid.Empty);

        // Assert
        result.Should().BeEmpty();
        _mockPostRepository.Verify(r => r.GetFeedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region GetPublicTimelineAsync Tests

    [Fact]
    public async Task GetPublicTimelineAsync_ShouldReturnPublicPosts()
    {
        // Arrange
        var limit = 10;
        var offset = 0;
        var expectedPosts = new List<Post>
        {
            new Post(Guid.NewGuid(), "Public Post 1"),
            new Post(Guid.NewGuid(), "Public Post 2")
        };
        
        _mockPostRepository.Setup(r => r.GetPublicTimelineAsync(limit, offset))
            .ReturnsAsync(expectedPosts);

        // Act
        var result = await _postService.GetPublicTimelineAsync(limit, offset);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedPosts);
        
        _mockPostRepository.Verify(r => r.GetPublicTimelineAsync(limit, offset), Times.Once);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task GetPublicTimelineAsync_WithInvalidLimit_ShouldUseDefaultLimit(int limit)
    {
        // Arrange
        var expectedPosts = new List<Post> { new Post(Guid.NewGuid(), "Post") };
        
        _mockPostRepository.Setup(r => r.GetPublicTimelineAsync(20, 0))
            .ReturnsAsync(expectedPosts);

        // Act
        var result = await _postService.GetPublicTimelineAsync(limit);

        // Assert
        result.Should().BeEquivalentTo(expectedPosts);
        _mockPostRepository.Verify(r => r.GetPublicTimelineAsync(20, 0), Times.Once);
    }

    #endregion

    #region SearchPostsAsync Tests

    [Fact]
    public async Task SearchPostsAsync_WithValidSearchTerm_ShouldReturnMatchingPosts()
    {
        // Arrange
        var searchTerm = "test";
        var viewerId = Guid.NewGuid();
        var limit = 10;
        var expectedPosts = new List<Post>
        {
            new Post(Guid.NewGuid(), "This is a test post"),
            new Post(Guid.NewGuid(), "Another test content")
        };
        
        _mockPostRepository.Setup(r => r.SearchAsync(searchTerm, limit, It.IsAny<int>()))
            .ReturnsAsync(expectedPosts);

        // Act
        var result = await _postService.SearchPostsAsync(searchTerm, viewerId, limit);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedPosts);
        
        _mockPostRepository.Verify(r => r.SearchAsync(searchTerm, limit, It.IsAny<int>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task SearchPostsAsync_WithInvalidSearchTerm_ShouldReturnEmpty(string searchTerm)
    {
        // Act
        var result = await _postService.SearchPostsAsync(searchTerm, Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
        _mockPostRepository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region CanUserViewPostAsync Tests

    [Fact]
    public async Task CanUserViewPostAsync_WithPublicPost_ShouldReturnTrue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var post = new Post(Guid.NewGuid(), "Public content");
        post.SetVisibility(PostVisibility.Public);
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);

        // Act
        var result = await _postService.CanUserViewPostAsync(postId, viewerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserViewPostAsync_WithEmptyPostId_ShouldReturnFalse()
    {
        // Act
        var result = await _postService.CanUserViewPostAsync(Guid.Empty, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _mockPostRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CanUserViewPostAsync_WithNonExistentPost_ShouldReturnFalse()
    {
        // Arrange
        var postId = Guid.NewGuid();
        
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _postService.CanUserViewPostAsync(postId, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetMediaPostsAsync Tests

    [Fact]
    public async Task GetMediaPostsAsync_ShouldReturnMediaPosts()
    {
        // Arrange
        var viewerId = Guid.NewGuid();
        var limit = 10;
        var offset = 0;
        var expectedPosts = new List<Post>
        {
            new Post(Guid.NewGuid(), "Media Post 1"),
            new Post(Guid.NewGuid(), "Media Post 2")
        };
        
        _mockPostRepository.Setup(r => r.GetPostsWithMediaAsync(limit, offset))
            .ReturnsAsync(expectedPosts);

        // Act
        var result = await _postService.GetMediaPostsAsync(viewerId, limit, offset);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedPosts);
        
        _mockPostRepository.Verify(r => r.GetPostsWithMediaAsync(limit, offset), Times.Once);
    }

    #endregion

    #region GetPostRepliesAsync Tests

    [Fact]
    public async Task GetPostRepliesAsync_WithValidParentPostId_ShouldReturnReplies()
    {
        // Arrange
        var parentPostId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var limit = 10;
        var offset = 0;
        var expectedReplies = new List<Post>
        {
            new Post(Guid.NewGuid(), "Reply 1", parentPostId),
            new Post(Guid.NewGuid(), "Reply 2", parentPostId)
        };
        
        _mockPostRepository.Setup(r => r.GetRepliesAsync(parentPostId, limit, offset))
            .ReturnsAsync(expectedReplies);

        // Act
        var result = await _postService.GetPostRepliesAsync(parentPostId, viewerId, limit, offset);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedReplies);
        
        _mockPostRepository.Verify(r => r.GetRepliesAsync(parentPostId, limit, offset), Times.Once);
    }

    [Fact]
    public async Task GetPostRepliesAsync_WithEmptyParentPostId_ShouldReturnEmpty()
    {
        // Act
        var result = await _postService.GetPostRepliesAsync(Guid.Empty, Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
        _mockPostRepository.Verify(r => r.GetRepliesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion
}
