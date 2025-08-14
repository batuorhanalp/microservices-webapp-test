using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Entities;
using WebApp.Core.Interfaces;
using WebApp.Infrastructure.Data;
using WebApp.Infrastructure.Repositories;
using Xunit;

namespace WebApp.Tests.Infrastructure.Repositories;

public class LikeRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ILikeRepository _likeRepository;

    public LikeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _likeRepository = new LikeRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnLike()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var like = new Like(user.Id, post.Id);
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();

        // Act
        var result = await _likeRepository.GetByIdAsync(like.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(like.Id);
        result.UserId.Should().Be(user.Id);
        result.PostId.Should().Be(post.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _likeRepository.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserAndPostAsync_WithValidIds_ShouldReturnLike()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var like = new Like(user.Id, post.Id);
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();

        // Act
        var result = await _likeRepository.GetByUserAndPostAsync(user.Id, post.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.PostId.Should().Be(post.Id);
    }

    [Fact]
    public async Task GetByUserAndPostAsync_WithNonexistentLike_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = await _likeRepository.GetByUserAndPostAsync(userId, postId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPostAsync_WithValidPostId_ShouldReturnLikesForPost()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1");
        var user2 = new User("user2@example.com", "user2", "User 2");
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var post1 = new Post(user1.Id, "Post 1");
        var post2 = new Post(user1.Id, "Post 2");
        await _context.Posts.AddRangeAsync(post1, post2);
        await _context.SaveChangesAsync();

        var likes = new[]
        {
            new Like(user1.Id, post1.Id),
            new Like(user2.Id, post1.Id),
            new Like(user1.Id, post2.Id)
        };
        await _context.Likes.AddRangeAsync(likes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _likeRepository.GetByPostAsync(post1.Id, 10, 0);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(l => l.PostId == post1.Id);
    }

    [Fact]
    public async Task GetByUserAsync_WithValidUserId_ShouldReturnLikesByUser()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1");
        var user2 = new User("user2@example.com", "user2", "User 2");
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var post1 = new Post(user1.Id, "Post 1");
        var post2 = new Post(user1.Id, "Post 2");
        await _context.Posts.AddRangeAsync(post1, post2);
        await _context.SaveChangesAsync();

        var likes = new[]
        {
            new Like(user1.Id, post1.Id),
            new Like(user1.Id, post2.Id),
            new Like(user2.Id, post1.Id)
        };
        await _context.Likes.AddRangeAsync(likes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _likeRepository.GetByUserAsync(user1.Id, 10, 0);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(l => l.UserId == user1.Id);
        result.Should().BeInDescendingOrder(l => l.CreatedAt);
    }

    [Fact]
    public async Task AddAsync_WithValidLike_ShouldAddLikeToDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var like = new Like(user.Id, post.Id);

        // Act
        await _likeRepository.AddAsync(like);
        await _likeRepository.SaveChangesAsync();

        // Assert
        var savedLike = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == user.Id && l.PostId == post.Id);
        savedLike.Should().NotBeNull();
        savedLike!.UserId.Should().Be(user.Id);
        savedLike.PostId.Should().Be(post.Id);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldRemoveLikeFromDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var like = new Like(user.Id, post.Id);
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();

        // Act
        await _likeRepository.DeleteAsync(like.Id);
        await _likeRepository.SaveChangesAsync();

        // Assert
        var deletedLike = await _context.Likes.FirstOrDefaultAsync(l => l.Id == like.Id);
        deletedLike.Should().BeNull();
    }

    [Fact]
    public async Task DeleteByUserAndPostAsync_WithValidIds_ShouldRemoveLikeFromDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var like = new Like(user.Id, post.Id);
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear(); // Clear tracking to avoid conflicts

        // Act
        await _likeRepository.DeleteByUserAndPostAsync(user.Id, post.Id);
        await _likeRepository.SaveChangesAsync();

        // Assert
        var deletedLike = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == user.Id && l.PostId == post.Id);
        deletedLike.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingLike_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var like = new Like(user.Id, post.Id);
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();

        // Act
        var result = await _likeRepository.ExistsAsync(user.Id, post.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonexistentLike_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = await _likeRepository.ExistsAsync(userId, postId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
