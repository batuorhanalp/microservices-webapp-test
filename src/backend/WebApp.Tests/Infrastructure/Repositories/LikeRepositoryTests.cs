using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
using WebApp.Common.Data;
using WebApp.Common.Repositories;
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
        var user = new User("user@example.com", "user", "User", "hashedpassword");
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
        var user = new User("user@example.com", "user", "User", "hashedpassword");
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
        var user1 = new User("user1@example.com", "user1", "User 1", "hashedpassword");
        var user2 = new User("user2@example.com", "user2", "User 2", "hashedpassword");
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
        var user1 = new User("user1@example.com", "user1", "User 1", "hashedpassword");
        var user2 = new User("user2@example.com", "user2", "User 2", "hashedpassword");
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
        var user = new User("user@example.com", "user", "User", "hashedpassword");
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
        var user = new User("user@example.com", "user", "User", "hashedpassword");
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
        var user = new User("user@example.com", "user", "User", "hashedpassword");
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
        var user = new User("user@example.com", "user", "User", "hashedpassword");
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

    [Fact]
    public async Task AddAsync_WithNullLike_ShouldThrowArgumentNullException()
    {
        // Arrange
        Like? nullLike = null;

        // Act & Assert
        await FluentActions.Invoking(() => _likeRepository.AddAsync(nullLike!))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("like");
    }

    [Fact]
    public async Task GetCountByPostAsync_WithValidPostId_ShouldReturnCorrectCount()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1", "hashedpassword");
        var user2 = new User("user2@example.com", "user2", "User 2", "hashedpassword");
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
        var result = await _likeRepository.GetCountByPostAsync(post1.Id);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCountByPostAsync_WithNonexistentPost_ShouldReturnZero()
    {
        // Arrange
        var nonExistentPostId = Guid.NewGuid();

        // Act
        var result = await _likeRepository.GetCountByPostAsync(nonExistentPostId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetCountByUserAsync_WithValidUserId_ShouldReturnCorrectCount()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1", "hashedpassword");
        var user2 = new User("user2@example.com", "user2", "User 2", "hashedpassword");
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
        var result = await _likeRepository.GetCountByUserAsync(user1.Id);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCountByUserAsync_WithNonexistentUser_ShouldReturnZero()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await _likeRepository.GetCountByUserAsync(nonExistentUserId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetUsersByPostAsync_WithValidPostId_ShouldReturnUsersWhoLikedPost()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1", "hashedpassword");
        var user2 = new User("user2@example.com", "user2", "User 2", "hashedpassword");
        var user3 = new User("user3@example.com", "user3", "User 3", "hashedpassword");
        await _context.Users.AddRangeAsync(user1, user2, user3);
        await _context.SaveChangesAsync();

        var post1 = new Post(user1.Id, "Post 1");
        var post2 = new Post(user1.Id, "Post 2");
        await _context.Posts.AddRangeAsync(post1, post2);
        await _context.SaveChangesAsync();

        var likes = new[]
        {
            new Like(user1.Id, post1.Id),
            new Like(user2.Id, post1.Id),
            new Like(user3.Id, post1.Id),
            new Like(user1.Id, post2.Id)
        };
        await _context.Likes.AddRangeAsync(likes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _likeRepository.GetUsersByPostAsync(post1.Id, 10, 0);

        // Assert
        result.Should().HaveCount(3);
        var userIds = result.Select(u => u.Id).ToList();
        userIds.Should().Contain(user1.Id);
        userIds.Should().Contain(user2.Id);
        userIds.Should().Contain(user3.Id);
    }

    [Fact]
    public async Task GetUsersByPostAsync_WithPagination_ShouldReturnCorrectSubset()
    {
        // Arrange
        var users = new List<User>();
        for (int i = 0; i < 5; i++)
        {
            users.Add(new User($"user{i}@example.com", $"user{i}", $"User {i}", "hashedpassword"));
        }
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        var post = new Post(users[0].Id, "Test Post");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var likes = users.Select(u => new Like(u.Id, post.Id)).ToArray();
        await _context.Likes.AddRangeAsync(likes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _likeRepository.GetUsersByPostAsync(post.Id, 2, 1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUsersByPostAsync_WithNonexistentPost_ShouldReturnEmptyCollection()
    {
        // Arrange
        var nonExistentPostId = Guid.NewGuid();

        // Act
        var result = await _likeRepository.GetUsersByPostAsync(nonExistentPostId, 10, 0);

        // Assert
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
