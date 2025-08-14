using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Entities;
using WebApp.Core.Interfaces;
using WebApp.Infrastructure.Data;
using WebApp.Infrastructure.Repositories;
using Xunit;

namespace WebApp.Tests.Infrastructure.Repositories;

public class PostRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IPostRepository _postRepository;

    public PostRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _postRepository = new PostRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnPost()
    {
        // Arrange
        var user = new User("author@example.com", "author", "Author");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Test post content");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetByIdAsync(post.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(post.Id);
        result.Content.Should().Be("Test post content");
        result.AuthorId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _postRepository.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByAuthorAsync_WithValidAuthorId_ShouldReturnAuthorPosts()
    {
        // Arrange
        var user1 = new User("author1@example.com", "author1", "Author 1");
        var user2 = new User("author2@example.com", "author2", "Author 2");
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var posts = new[]
        {
            new Post(user1.Id, "Post 1 by user1"),
            new Post(user1.Id, "Post 2 by user1"),
            new Post(user2.Id, "Post by user2")
        };
        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetByAuthorAsync(user1.Id, 10, 0);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.AuthorId == user1.Id);
    }

    [Fact]
    public async Task GetFeedAsync_WithFollowedUsers_ShouldReturnPostsFromFollowedUsers()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        var followed = new User("followed@example.com", "followed", "Followed");
        await _context.Users.AddRangeAsync(user, followed);
        await _context.SaveChangesAsync();

        var follow = new Follow(user.Id, followed.Id, false);
        await _context.Follows.AddAsync(follow);
        await _context.SaveChangesAsync();

        var post = new Post(followed.Id, "Post by followed user");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetFeedAsync(user.Id, 10, 0);

        // Assert
        result.Should().HaveCount(1);
        result.First().AuthorId.Should().Be(followed.Id);
    }

    [Fact]
    public async Task GetPublicTimelineAsync_ShouldReturnPublicPostsOrderedByDate()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1");
        var user2 = new User("user2@example.com", "user2", "User 2");
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var posts = new[]
        {
            new Post(user1.Id, "Public post 1", PostType.Text, PostVisibility.Public),
            new Post(user2.Id, "Private post", PostType.Text, PostVisibility.Private),
            new Post(user1.Id, "Public post 2", PostType.Text, PostVisibility.Public)
        };
        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetPublicTimelineAsync(10, 0);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Visibility == PostVisibility.Public);
    }

    [Fact]
    public async Task GetRepliesAsync_WithValidPostId_ShouldReturnReplies()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var parentPost = new Post(user.Id, "Parent post");
        await _context.Posts.AddAsync(parentPost);
        await _context.SaveChangesAsync();

        var replies = new[]
        {
            new Post(user.Id, "Reply 1", parentPost.Id),
            new Post(user.Id, "Reply 2", parentPost.Id)
        };
        await _context.Posts.AddRangeAsync(replies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetRepliesAsync(parentPost.Id, 10, 0);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.ParentPostId == parentPost.Id);
    }

    [Fact]
    public async Task AddAsync_WithValidPost_ShouldAddPostToDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "New post content");

        // Act
        await _postRepository.AddAsync(post);
        await _postRepository.SaveChangesAsync();

        // Assert
        var savedPost = await _context.Posts.FirstOrDefaultAsync(p => p.Content == "New post content");
        savedPost.Should().NotBeNull();
        savedPost!.AuthorId.Should().Be(user.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithValidPost_ShouldUpdatePostInDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Original content");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        // Modify post
        post.UpdateContent("Updated content");

        // Act
        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync();

        // Assert
        var updatedPost = await _context.Posts.FirstOrDefaultAsync(p => p.Id == post.Id);
        updatedPost.Should().NotBeNull();
        updatedPost!.Content.Should().Be("Updated content");
        updatedPost.IsEdited.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldRemovePostFromDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Post to delete");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        // Act
        await _postRepository.DeleteAsync(post.Id);
        await _postRepository.SaveChangesAsync();

        // Assert
        var deletedPost = await _context.Posts.FirstOrDefaultAsync(p => p.Id == post.Id);
        deletedPost.Should().BeNull();
    }

    [Fact]
    public async Task GetWithMediaAsync_WithValidPostId_ShouldReturnPost()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Post with media");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetWithMediaAsync(post.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(post.Id);
        result.Content.Should().Be("Post with media");
        // Media attachments will be tested in integration tests
    }

    [Fact]
    public async Task SearchAsync_WithSearchTerm_ShouldReturnMatchingPosts()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var posts = new[]
        {
            new Post(user.Id, "This contains searchterm in content"),
            new Post(user.Id, "Another post with searchterm here"),
            new Post(user.Id, "This post has different content")
        };
        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.SearchAsync("searchterm", 10, 0);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Content.Contains("searchterm"));
    }

    [Fact]
    public async Task GetTrendingAsync_ShouldReturnPostsOrderedByEngagement()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var posts = new[]
        {
            new Post(user.Id, "Trending post 1"),
            new Post(user.Id, "Trending post 2")
        };
        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();

        // Add some likes to make posts trending (simplified for test)
        var likes = new[]
        {
            new Like(user.Id, posts[0].Id),
            new Like(user.Id, posts[1].Id)
        };
        await _context.Likes.AddRangeAsync(likes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetTrendingAsync(10, 0);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLikeCountAsync_WithValidPostId_ShouldReturnCorrectCount()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1");
        var user2 = new User("user2@example.com", "user2", "User 2");
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var post = new Post(user1.Id, "Post with likes");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var likes = new[]
        {
            new Like(user1.Id, post.Id),
            new Like(user2.Id, post.Id)
        };
        await _context.Likes.AddRangeAsync(likes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetLikeCountAsync(post.Id);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCommentCountAsync_WithValidPostId_ShouldReturnCorrectCount()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var post = new Post(user.Id, "Post with comments");
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var comments = new[]
        {
            new Comment(user.Id, post.Id, "Comment 1"),
            new Comment(user.Id, post.Id, "Comment 2")
        };
        await _context.Comments.AddRangeAsync(comments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetCommentCountAsync(post.Id);

        // Assert
        result.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
