using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Entities;
using WebApp.Core.Interfaces;
using WebApp.Infrastructure.Data;
using WebApp.Infrastructure.Repositories;
using Xunit;

namespace WebApp.Tests.Infrastructure.Repositories;

public class CommentRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ICommentRepository _commentRepository;

    public CommentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _commentRepository = new CommentRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnComment()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var comment = new Comment(user.Id, post.Id, "Test comment");
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _commentRepository.GetByIdAsync(comment.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(comment.Id);
        result.Content.Should().Be("Test comment");
        result.UserId.Should().Be(user.Id);
        result.PostId.Should().Be(post.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _commentRepository.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPostAsync_WithValidPostId_ShouldReturnCommentsOrderedByDate()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post1 = new Post(user.Id, "Post 1");
        var post2 = new Post(user.Id, "Post 2");
        await _context.Posts.AddRangeAsync(post1, post2);
        await _context.SaveChangesAsync();

        var comments = new[]
        {
            new Comment(user.Id, post1.Id, "Comment 1 on post 1"),
            new Comment(user.Id, post1.Id, "Comment 2 on post 1"),
            new Comment(user.Id, post2.Id, "Comment on post 2")
        };
        await _context.Comments.AddRangeAsync(comments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _commentRepository.GetByPostAsync(post1.Id, 10, 0);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.PostId == post1.Id);
        result.Should().BeInAscendingOrder(c => c.CreatedAt);
    }

    [Fact]
    public async Task GetByAuthorAsync_WithValidAuthorId_ShouldReturnAuthorComments()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1");
        var user2 = new User("user2@example.com", "user2", "User 2");
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var post = new Post(user1.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var comments = new[]
        {
            new Comment(user1.Id, post.Id, "Comment by user1"),
            new Comment(user1.Id, post.Id, "Another comment by user1"),
            new Comment(user2.Id, post.Id, "Comment by user2")
        };
        await _context.Comments.AddRangeAsync(comments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _commentRepository.GetByAuthorAsync(user1.Id, 10, 0);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.UserId == user1.Id);
    }


    [Fact]
    public async Task AddAsync_WithValidComment_ShouldAddCommentToDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var comment = new Comment(user.Id, post.Id, "New comment");

        // Act
        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        // Assert
        var savedComment = await _context.Comments.FirstOrDefaultAsync(c => c.Content == "New comment");
        savedComment.Should().NotBeNull();
        savedComment!.UserId.Should().Be(user.Id);
        savedComment.PostId.Should().Be(post.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithValidComment_ShouldUpdateCommentInDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var comment = new Comment(user.Id, post.Id, "Original content");
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        // Modify comment
        comment.UpdateContent("Updated content");

        // Act
        _commentRepository.Update(comment);
        await _commentRepository.SaveChangesAsync();

        // Assert
        var updatedComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id);
        updatedComment.Should().NotBeNull();
        updatedComment!.Content.Should().Be("Updated content");
        updatedComment.IsEdited.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldRemoveCommentFromDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var comment = new Comment(user.Id, post.Id, "Comment to delete");
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        // Act
        await _commentRepository.DeleteAsync(comment.Id);
        await _commentRepository.SaveChangesAsync();

        // Assert
        var deletedComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id);
        deletedComment.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
