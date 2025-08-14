using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
using WebApp.Common.Services;
using Xunit;

namespace WebApp.Tests.Application.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _mockCommentRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPostRepository> _mockPostRepository;
    private readonly Mock<ILogger<CommentService>> _mockLogger;
    private readonly ICommentService _commentService;

    public CommentServiceTests()
    {
        _mockCommentRepository = new Mock<ICommentRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPostRepository = new Mock<IPostRepository>();
        _mockLogger = new Mock<ILogger<CommentService>>();
        _commentService = new CommentService(
            _mockCommentRepository.Object,
            _mockUserRepository.Object,
            _mockPostRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullCommentRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new CommentService(
            null!,
            _mockUserRepository.Object,
            _mockPostRepository.Object,
            _mockLogger.Object);
        
        act.Should().Throw<ArgumentNullException>().WithParameterName("commentRepository");
    }

    [Fact]
    public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new CommentService(
            _mockCommentRepository.Object,
            null!,
            _mockPostRepository.Object,
            _mockLogger.Object);
        
        act.Should().Throw<ArgumentNullException>().WithParameterName("userRepository");
    }

    [Fact]
    public void Constructor_WithNullPostRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new CommentService(
            _mockCommentRepository.Object,
            _mockUserRepository.Object,
            null!,
            _mockLogger.Object);
        
        act.Should().Throw<ArgumentNullException>().WithParameterName("postRepository");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new CommentService(
            _mockCommentRepository.Object,
            _mockUserRepository.Object,
            _mockPostRepository.Object,
            null!);
        
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task CreateCommentAsync_WithValidData_ShouldCreateComment()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var content = "This is a test comment";
        
        var author = new User("author@example.com", "author", "Author", "hashedpassword");
        var post = new Post(authorId, "Test post");
        var comment = new Comment(authorId, postId, content);

        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);
        _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);
        _mockCommentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _commentService.CreateCommentAsync(authorId, postId, content);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(authorId);
        result.PostId.Should().Be(postId);
        result.Content.Should().Be(content);

        _mockCommentRepository.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        _mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCommentAsync_WithEmptyAuthorId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateCommentAsync(Guid.Empty, Guid.NewGuid(), "Test content"));
        
        exception.ParamName.Should().Be("authorId");
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    [Fact]
    public async Task CreateCommentAsync_WithEmptyPostId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateCommentAsync(Guid.NewGuid(), Guid.Empty, "Test content"));
        
        exception.ParamName.Should().Be("postId");
        exception.Message.Should().Contain("Post ID cannot be empty");
    }

    [Fact]
    public async Task CreateCommentAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateCommentAsync(Guid.NewGuid(), Guid.NewGuid(), ""));
        
        exception.ParamName.Should().Be("content");
        exception.Message.Should().Contain("Comment content is required");
    }

    [Fact]
    public async Task CreateCommentAsync_WithWhitespaceContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateCommentAsync(Guid.NewGuid(), Guid.NewGuid(), "   "));
        
        exception.ParamName.Should().Be("content");
        exception.Message.Should().Contain("Comment content is required");
    }

    [Fact]
    public async Task CreateCommentAsync_WithNonExistentAuthor_ShouldThrowArgumentException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateCommentAsync(authorId, postId, "Test content"));
        
        exception.ParamName.Should().Be("authorId");
        exception.Message.Should().Contain("Author not found");
    }

    [Fact]
    public async Task CreateCommentAsync_WithNonExistentPost_ShouldThrowArgumentException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var author = new User("author@example.com", "author", "Author", "hashedpassword");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _mockPostRepository.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateCommentAsync(authorId, postId, "Test content"));
        
        exception.ParamName.Should().Be("postId");
        exception.Message.Should().Contain("Post not found");
    }

    [Fact]
    public async Task CreateReplyAsync_WithValidData_ShouldCreateReply()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var content = "This is a test reply";
        
        var author = new User("author@example.com", "author", "Author", "hashedpassword");
        var parentComment = new Comment(Guid.NewGuid(), postId, "Parent comment");

        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _mockCommentRepository.Setup(r => r.GetByIdAsync(parentCommentId))
            .ReturnsAsync(parentComment);
        _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);
        _mockCommentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _commentService.CreateReplyAsync(authorId, parentCommentId, content);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(authorId);
        result.PostId.Should().Be(postId);
        result.Content.Should().Be(content);

        _mockCommentRepository.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        _mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateReplyAsync_WithEmptyAuthorId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateReplyAsync(Guid.Empty, Guid.NewGuid(), "Test content"));
        
        exception.ParamName.Should().Be("authorId");
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    [Fact]
    public async Task CreateReplyAsync_WithEmptyParentCommentId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateReplyAsync(Guid.NewGuid(), Guid.Empty, "Test content"));
        
        exception.ParamName.Should().Be("parentCommentId");
        exception.Message.Should().Contain("Parent comment ID cannot be empty");
    }

    [Fact]
    public async Task CreateReplyAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateReplyAsync(Guid.NewGuid(), Guid.NewGuid(), ""));
        
        exception.ParamName.Should().Be("content");
        exception.Message.Should().Contain("Reply content is required");
    }

    [Fact]
    public async Task CreateReplyAsync_WithNonExistentAuthor_ShouldThrowArgumentException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateReplyAsync(authorId, parentCommentId, "Test content"));
        
        exception.ParamName.Should().Be("authorId");
        exception.Message.Should().Contain("Author not found");
    }

    [Fact]
    public async Task CreateReplyAsync_WithNonExistentParentComment_ShouldThrowArgumentException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var author = new User("author@example.com", "author", "Author", "hashedpassword");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(authorId))
            .ReturnsAsync(author);
        _mockCommentRepository.Setup(r => r.GetByIdAsync(parentCommentId))
            .ReturnsAsync((Comment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.CreateReplyAsync(authorId, parentCommentId, "Test content"));
        
        exception.ParamName.Should().Be("parentCommentId");
        exception.Message.Should().Contain("Parent comment not found");
    }

    [Fact]
    public async Task UpdateCommentAsync_WithValidData_ShouldUpdateComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var newContent = "Updated comment content";
        
        var comment = new Comment(authorId, Guid.NewGuid(), "Original content");

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(comment);
        _mockCommentRepository.Setup(r => r.Update(comment));
        _mockCommentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _commentService.UpdateCommentAsync(commentId, authorId, newContent);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(newContent);
        result.IsEdited.Should().BeTrue();

        _mockCommentRepository.Verify(r => r.Update(comment), Times.Once);
        _mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCommentAsync_WithEmptyCommentId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.UpdateCommentAsync(Guid.Empty, Guid.NewGuid(), "New content"));
        
        exception.ParamName.Should().Be("commentId");
        exception.Message.Should().Contain("Comment ID cannot be empty");
    }

    [Fact]
    public async Task UpdateCommentAsync_WithEmptyAuthorId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.UpdateCommentAsync(Guid.NewGuid(), Guid.Empty, "New content"));
        
        exception.ParamName.Should().Be("authorId");
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    [Fact]
    public async Task UpdateCommentAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.UpdateCommentAsync(Guid.NewGuid(), Guid.NewGuid(), ""));
        
        exception.ParamName.Should().Be("newContent");
        exception.Message.Should().Contain("Comment content cannot be empty");
    }

    [Fact]
    public async Task UpdateCommentAsync_WithNonExistentComment_ShouldThrowArgumentException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        
        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.UpdateCommentAsync(commentId, authorId, "New content"));
        
        exception.ParamName.Should().Be("commentId");
        exception.Message.Should().Contain("Comment not found");
    }

    [Fact]
    public async Task UpdateCommentAsync_WithUnauthorizedUser_ShouldThrowArgumentException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();
        
        var comment = new Comment(authorId, Guid.NewGuid(), "Original content");

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.UpdateCommentAsync(commentId, unauthorizedUserId, "New content"));
        
        exception.ParamName.Should().Be("authorId");
        exception.Message.Should().Contain("User is not authorized to update this comment");
    }

    [Fact]
    public async Task DeleteCommentAsync_WithValidData_ShouldDeleteComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        
        var comment = new Comment(authorId, Guid.NewGuid(), "Comment to delete");

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(comment);
        _mockCommentRepository.Setup(r => r.DeleteAsync(commentId))
            .Returns(Task.CompletedTask);
        _mockCommentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _commentService.DeleteCommentAsync(commentId, authorId);

        // Assert
        result.Should().BeTrue();

        _mockCommentRepository.Verify(r => r.DeleteAsync(commentId), Times.Once);
        _mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_WithEmptyCommentId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.DeleteCommentAsync(Guid.Empty, Guid.NewGuid()));
        
        exception.ParamName.Should().Be("commentId");
        exception.Message.Should().Contain("Comment ID cannot be empty");
    }

    [Fact]
    public async Task DeleteCommentAsync_WithEmptyAuthorId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.DeleteCommentAsync(Guid.NewGuid(), Guid.Empty));
        
        exception.ParamName.Should().Be("authorId");
        exception.Message.Should().Contain("Author ID cannot be empty");
    }

    [Fact]
    public async Task DeleteCommentAsync_WithNonExistentComment_ShouldReturnFalse()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        
        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        // Act
        var result = await _commentService.DeleteCommentAsync(commentId, authorId);

        // Assert
        result.Should().BeFalse();
        _mockCommentRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentAsync_WithUnauthorizedUser_ShouldThrowArgumentException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();
        
        var comment = new Comment(authorId, Guid.NewGuid(), "Comment to delete");

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _commentService.DeleteCommentAsync(commentId, unauthorizedUserId));
        
        exception.ParamName.Should().Be("authorId");
        exception.Message.Should().Contain("User is not authorized to delete this comment");
    }

    [Fact]
    public async Task GetCommentByIdAsync_WithValidId_ShouldReturnComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "Test comment");

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        // Act
        var result = await _commentService.GetCommentByIdAsync(commentId, viewerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(comment);
    }

    [Fact]
    public async Task GetCommentByIdAsync_WithEmptyId_ShouldReturnNull()
    {
        // Act
        var result = await _commentService.GetCommentByIdAsync(Guid.Empty);

        // Assert
        result.Should().BeNull();
        _mockCommentRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetCommentByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var commentId = Guid.NewGuid();

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        // Act
        var result = await _commentService.GetCommentByIdAsync(commentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPostCommentsAsync_WithValidPostId_ShouldReturnComments()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new Comment(Guid.NewGuid(), postId, "Comment 1"),
            new Comment(Guid.NewGuid(), postId, "Comment 2")
        };

        _mockCommentRepository.Setup(r => r.GetByPostAsync(postId, 50, 0))
            .ReturnsAsync(comments);

        // Act
        var result = await _commentService.GetPostCommentsAsync(postId);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().OnlyContain(c => c.PostId == postId);
    }

    [Fact]
    public async Task GetPostCommentsAsync_WithEmptyPostId_ShouldReturnEmpty()
    {
        // Act
        var result = await _commentService.GetPostCommentsAsync(Guid.Empty);

        // Assert
        result.Should().BeEmpty();
        _mockCommentRepository.Verify(r => r.GetByPostAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPostCommentsAsync_WithInvalidLimit_ShouldUseDefaultLimit()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var comments = new List<Comment>();

        _mockCommentRepository.Setup(r => r.GetByPostAsync(postId, 50, 0))
            .ReturnsAsync(comments);

        // Act
        await _commentService.GetPostCommentsAsync(postId, null, -1);

        // Assert
        _mockCommentRepository.Verify(r => r.GetByPostAsync(postId, 50, 0), Times.Once);
    }

    [Fact]
    public async Task GetPostCommentsAsync_WithNegativeOffset_ShouldUseZeroOffset()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var comments = new List<Comment>();

        _mockCommentRepository.Setup(r => r.GetByPostAsync(postId, 50, 0))
            .ReturnsAsync(comments);

        // Act
        await _commentService.GetPostCommentsAsync(postId, null, 50, -5);

        // Assert
        _mockCommentRepository.Verify(r => r.GetByPostAsync(postId, 50, 0), Times.Once);
    }

    [Fact]
    public async Task GetPostCommentsAsync_WithCustomLimitAndOffset_ShouldUseProvidedValues()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var comments = new List<Comment>();

        _mockCommentRepository.Setup(r => r.GetByPostAsync(postId, 20, 10))
            .ReturnsAsync(comments);

        // Act
        await _commentService.GetPostCommentsAsync(postId, null, 20, 10);

        // Assert
        _mockCommentRepository.Verify(r => r.GetByPostAsync(postId, 20, 10), Times.Once);
    }
}
