using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Entities;
using WebApp.Core.Interfaces;
using WebApp.Infrastructure.Data;
using WebApp.Infrastructure.Repositories;
using Xunit;

namespace WebApp.Tests.Infrastructure.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _userRepository;

    public UserRepositoryTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange - RED: This test will fail because UserRepository doesn't exist yet
        var user = new User("test@example.com", "testuser", "Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.Username.Should().Be("testuser");
        result.DisplayName.Should().Be("Test User");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _userRepository.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        var user = new User("unique@example.com", "uniqueuser", "Unique User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByEmailAsync("unique@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("unique@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Act
        var result = await _userRepository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithValidUsername_ShouldReturnUser()
    {
        // Arrange
        var user = new User("username@example.com", "testusername", "Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByUsernameAsync("testusername");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testusername");
    }

    [Fact]
    public async Task GetByUsernameAsync_WithInvalidUsername_ShouldReturnNull()
    {
        // Act
        var result = await _userRepository.GetByUsernameAsync("nonexistentuser");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WithValidUser_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = new User("add@example.com", "adduser", "Add User");

        // Act
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "add@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be("adduser");
    }

    [Fact]
    public async Task UpdateAsync_WithValidUser_ShouldUpdateUserInDatabase()
    {
        // Arrange
        var user = new User("update@example.com", "updateuser", "Original Name");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Modify user
        user.UpdateProfile("Updated Name", "Updated bio", "https://example.com", "Updated Location");

        // Act
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        // Assert
        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.DisplayName.Should().Be("Updated Name");
        updatedUser.Bio.Should().Be("Updated bio");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var user = new User("delete@example.com", "deleteuser", "Delete User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        await _userRepository.DeleteAsync(user.Id);
        await _userRepository.SaveChangesAsync();

        // Assert
        var deletedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetFollowersAsync_WithValidUserId_ShouldReturnFollowers()
    {
        // Arrange
        var user = new User("followed@example.com", "followed", "Followed User");
        var follower = new User("follower@example.com", "follower", "Follower User");
        await _context.Users.AddRangeAsync(user, follower);
        await _context.SaveChangesAsync();

        var follow = new Follow(follower.Id, user.Id, false);
        await _context.Follows.AddAsync(follow);
        await _context.SaveChangesAsync();

        // Act
        var followers = await _userRepository.GetFollowersAsync(user.Id);

        // Assert
        followers.Should().HaveCount(1);
        followers.First().Id.Should().Be(follower.Id);
    }

    [Fact]
    public async Task GetFollowingAsync_WithValidUserId_ShouldReturnFollowing()
    {
        // Arrange
        var user = new User("follower@example.com", "follower", "Follower User");
        var followed = new User("followed@example.com", "followed", "Followed User");
        await _context.Users.AddRangeAsync(user, followed);
        await _context.SaveChangesAsync();

        var follow = new Follow(user.Id, followed.Id, false);
        await _context.Follows.AddAsync(follow);
        await _context.SaveChangesAsync();

        // Act
        var following = await _userRepository.GetFollowingAsync(user.Id);

        // Assert
        following.Should().HaveCount(1);
        following.First().Id.Should().Be(followed.Id);
    }

    [Fact]
    public async Task SearchByUsernameAsync_WithPartialMatch_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new[]
        {
            new User("search1@example.com", "searchuser1", "Search User 1"),
            new User("search2@example.com", "searchuser2", "Search User 2"),
            new User("other@example.com", "othername", "Other User")
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var results = await _userRepository.SearchByUsernameAsync("search", 10);

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(u => u.Username.Contains("search"));
    }

    [Fact]
    public async Task IsEmailTakenAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("taken@example.com", "takenuser", "Taken User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.IsEmailTakenAsync("taken@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailTakenAsync_WithNewEmail_ShouldReturnFalse()
    {
        // Act
        var result = await _userRepository.IsEmailTakenAsync("available@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsUsernameTakenAsync_WithExistingUsername_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("username@example.com", "takenusername", "Taken Username");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.IsUsernameTakenAsync("takenusername");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUsernameTakenAsync_WithNewUsername_ShouldReturnFalse()
    {
        // Act
        var result = await _userRepository.IsUsernameTakenAsync("availableusername");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByEmailAsync_WithNullOrWhitespaceEmail_ShouldReturnNull()
    {
        // Act & Assert - Test null
        var resultNull = await _userRepository.GetByEmailAsync(null!);
        resultNull.Should().BeNull();

        // Act & Assert - Test empty string
        var resultEmpty = await _userRepository.GetByEmailAsync("");
        resultEmpty.Should().BeNull();

        // Act & Assert - Test whitespace
        var resultWhitespace = await _userRepository.GetByEmailAsync("   ");
        resultWhitespace.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNullOrWhitespaceUsername_ShouldReturnNull()
    {
        // Act & Assert - Test null
        var resultNull = await _userRepository.GetByUsernameAsync(null!);
        resultNull.Should().BeNull();

        // Act & Assert - Test empty string
        var resultEmpty = await _userRepository.GetByUsernameAsync("");
        resultEmpty.Should().BeNull();

        // Act & Assert - Test whitespace
        var resultWhitespace = await _userRepository.GetByUsernameAsync("   ");
        resultWhitespace.Should().BeNull();
    }

    [Fact]
    public async Task SearchByUsernameAsync_WithNullOrWhitespaceSearchTerm_ShouldReturnEmpty()
    {
        // Act & Assert - Test null
        var resultNull = await _userRepository.SearchByUsernameAsync(null!);
        resultNull.Should().BeEmpty();

        // Act & Assert - Test empty string
        var resultEmpty = await _userRepository.SearchByUsernameAsync("");
        resultEmpty.Should().BeEmpty();

        // Act & Assert - Test whitespace
        var resultWhitespace = await _userRepository.SearchByUsernameAsync("   ");
        resultWhitespace.Should().BeEmpty();
    }

    [Fact]
    public async Task IsEmailTakenAsync_WithNullOrWhitespaceEmail_ShouldReturnFalse()
    {
        // Act & Assert - Test null
        var resultNull = await _userRepository.IsEmailTakenAsync(null!);
        resultNull.Should().BeFalse();

        // Act & Assert - Test empty string
        var resultEmpty = await _userRepository.IsEmailTakenAsync("");
        resultEmpty.Should().BeFalse();

        // Act & Assert - Test whitespace
        var resultWhitespace = await _userRepository.IsEmailTakenAsync("   ");
        resultWhitespace.Should().BeFalse();
    }

    [Fact]
    public async Task IsUsernameTakenAsync_WithNullOrWhitespaceUsername_ShouldReturnFalse()
    {
        // Act & Assert - Test null
        var resultNull = await _userRepository.IsUsernameTakenAsync(null!);
        resultNull.Should().BeFalse();

        // Act & Assert - Test empty string
        var resultEmpty = await _userRepository.IsUsernameTakenAsync("");
        resultEmpty.Should().BeFalse();

        // Act & Assert - Test whitespace
        var resultWhitespace = await _userRepository.IsUsernameTakenAsync("   ");
        resultWhitespace.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _userRepository.AddAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Update_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _userRepository.Update(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - Should not throw
        var act = async () => await _userRepository.DeleteAsync(nonExistentId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Constructor_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new UserRepository(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*context*");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
