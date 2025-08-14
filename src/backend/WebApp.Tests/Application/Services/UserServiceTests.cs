using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Application.Services;
using WebApp.Core.Entities;
using WebApp.Core.Interfaces;
using Xunit;

namespace WebApp.Tests.Application.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockUserRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = "newuser@example.com";
        var username = "newuser";
        var displayName = "New User";
        
        _mockUserRepository.Setup(r => r.IsEmailTakenAsync(email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.IsUsernameTakenAsync(username))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.CreateUserAsync(email, username, displayName);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Username.Should().Be(username);
        result.DisplayName.Should().Be(displayName);
        
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithTakenEmail_ShouldThrowException()
    {
        // Arrange
        var email = "taken@example.com";
        var username = "newuser";
        var displayName = "New User";
        
        _mockUserRepository.Setup(r => r.IsEmailTakenAsync(email))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.CreateUserAsync(email, username, displayName));
        
        exception.Message.Should().Contain("Email is already taken");
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WithTakenUsername_ShouldThrowException()
    {
        // Arrange
        var email = "newuser@example.com";
        var username = "takenuser";
        var displayName = "New User";
        
        _mockUserRepository.Setup(r => r.IsEmailTakenAsync(email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.IsUsernameTakenAsync(username))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.CreateUserAsync(email, username, displayName));
        
        exception.Message.Should().Contain("Username is already taken");
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User("user@example.com", "user", "User");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedUser);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("user@example.com", "user", "User");
        var newDisplayName = "Updated User";
        var newBio = "Updated bio";
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateUserProfileAsync(userId, newDisplayName, newBio);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be(newDisplayName);
        result.Bio.Should().Be(newBio);
        
        _mockUserRepository.Verify(r => r.Update(user), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_WithNonExistentUser_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.UpdateUserProfileAsync(userId, "Name", "Bio"));
        
        exception.Message.Should().Contain("User not found");
        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task FollowUserAsync_WithValidUsers_ShouldCreateFollow()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followeeId = Guid.NewGuid();
        var follower = new User("follower@example.com", "follower", "Follower");
        var followee = new User("followee@example.com", "followee", "Followee");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(followerId))
            .ReturnsAsync(follower);
        _mockUserRepository.Setup(r => r.GetByIdAsync(followeeId))
            .ReturnsAsync(followee);
        _mockUserRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _userService.FollowUserAsync(followerId, followeeId);

        // Assert
        _mockUserRepository.Verify(r => r.GetByIdAsync(followerId), Times.Once);
        _mockUserRepository.Verify(r => r.GetByIdAsync(followeeId), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task FollowUserAsync_WithSameUser_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.FollowUserAsync(userId, userId));
        
        exception.Message.Should().Contain("cannot follow themselves");
    }

    [Fact]
    public async Task FollowUserAsync_WithNonExistentFollower_ShouldThrowException()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followeeId = Guid.NewGuid();
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(followerId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.FollowUserAsync(followerId, followeeId));
        
        exception.Message.Should().Contain("Follower not found");
    }

    [Fact]
    public async Task FollowUserAsync_WithNonExistentFollowee_ShouldThrowException()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followeeId = Guid.NewGuid();
        var follower = new User("follower@example.com", "follower", "Follower");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(followerId))
            .ReturnsAsync(follower);
        _mockUserRepository.Setup(r => r.GetByIdAsync(followeeId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.FollowUserAsync(followerId, followeeId));
        
        exception.Message.Should().Contain("User to follow not found");
    }

    [Fact]
    public async Task UnfollowUserAsync_WithValidUsers_ShouldRemoveFollow()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followeeId = Guid.NewGuid();
        var follower = new User("follower@example.com", "follower", "Follower");
        var followee = new User("followee@example.com", "followee", "Followee");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(followerId))
            .ReturnsAsync(follower);
        _mockUserRepository.Setup(r => r.GetByIdAsync(followeeId))
            .ReturnsAsync(followee);
        _mockUserRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _userService.UnfollowUserAsync(followerId, followeeId);

        // Assert
        _mockUserRepository.Verify(r => r.GetByIdAsync(followerId), Times.Once);
        _mockUserRepository.Verify(r => r.GetByIdAsync(followeeId), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchUsersAsync_WithValidTerm_ShouldReturnUsers()
    {
        // Arrange
        var searchTerm = "test";
        var expectedUsers = new List<User>
        {
            new User("test1@example.com", "test1", "Test User 1"),
            new User("test2@example.com", "test2", "Test User 2")
        };
        
        _mockUserRepository.Setup(r => r.SearchByUsernameAsync(searchTerm, 20))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.SearchUsersAsync(searchTerm);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedUsers);
        _mockUserRepository.Verify(r => r.SearchByUsernameAsync(searchTerm, 20), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task SearchUsersAsync_WithInvalidTerm_ShouldReturnEmpty(string searchTerm)
    {
        // Act
        var result = await _userService.SearchUsersAsync(searchTerm);

        // Assert
        result.Should().BeEmpty();
        _mockUserRepository.Verify(r => r.SearchByUsernameAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }
}
