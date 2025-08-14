using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Common.Services;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
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
    public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new UserService(null!, _mockLogger.Object));
        
        exception.ParamName.Should().Be("userRepository");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new UserService(_mockUserRepository.Object, null!));
        
        exception.ParamName.Should().Be("logger");
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
        
        exception.Message.Should().Contain("Email address is already in use");
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
        
        exception.Message.Should().Contain("Username is already in use");
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User("user@example.com", "user", "User", "hashedpassword");
        
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
        var user = new User("user@example.com", "user", "User", "hashedpassword");
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
        var follower = new User("follower@example.com", "follower", "Follower", "hashedpassword");
        var followee = new User("followee@example.com", "followee", "Followee", "hashedpassword");
        
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
        
        exception.Message.Should().Contain("Follower user not found");
    }

    [Fact]
    public async Task FollowUserAsync_WithNonExistentFollowee_ShouldThrowException()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followeeId = Guid.NewGuid();
        var follower = new User("follower@example.com", "follower", "Follower", "hashedpassword");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(followerId))
            .ReturnsAsync(follower);
        _mockUserRepository.Setup(r => r.GetByIdAsync(followeeId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.FollowUserAsync(followerId, followeeId));
        
        exception.Message.Should().Contain("Followee user not found");
    }

    [Fact]
    public async Task UnfollowUserAsync_WithValidUsers_ShouldRemoveFollow()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followeeId = Guid.NewGuid();
        var follower = new User("follower@example.com", "follower", "Follower", "hashedpassword");
        
        // Add a follow relationship to the follower
        var follow = new Follow(followerId, followeeId, false);
        follower.Following.Add(follow);
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(followerId))
            .ReturnsAsync(follower);
        _mockUserRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _userService.UnfollowUserAsync(followerId, followeeId);

        // Assert
        _mockUserRepository.Verify(r => r.GetByIdAsync(followerId), Times.Once);
        _mockUserRepository.Verify(r => r.Update(follower), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        
        // Verify the follow was removed
        follower.Following.Should().NotContain(f => f.FolloweeId == followeeId);
    }

    [Fact]
    public async Task SearchUsersAsync_WithValidTerm_ShouldReturnUsers()
    {
        // Arrange
        var searchTerm = "test";
        var expectedUsers = new List<User>
        {
            new User("test1@example.com", "test1", "Test User 1", "hashedpassword"),
            new User("test2@example.com", "test2", "Test User 2", "hashedpassword")
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

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task CreateUserAsync_WithInvalidEmail_ShouldThrowException(string email)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.CreateUserAsync(email, "username", "Display Name"));
        
        exception.Message.Should().Contain("Email is required");
        _mockUserRepository.Verify(r => r.IsEmailTakenAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task CreateUserAsync_WithInvalidUsername_ShouldThrowException(string username)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.CreateUserAsync("email@example.com", username, "Display Name"));
        
        exception.Message.Should().Contain("Username is required");
        _mockUserRepository.Verify(r => r.IsEmailTakenAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task CreateUserAsync_WithInvalidDisplayName_ShouldThrowException(string displayName)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.CreateUserAsync("email@example.com", "username", displayName));
        
        exception.Message.Should().Contain("Display name is required");
        _mockUserRepository.Verify(r => r.IsEmailTakenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithEmptyGuid_ShouldReturnNull()
    {
        // Act
        var result = await _userService.GetUserByIdAsync(Guid.Empty);

        // Assert
        result.Should().BeNull();
        _mockUserRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_WithEmptyUserId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.UpdateUserProfileAsync(Guid.Empty, "New Name"));
        
        exception.Message.Should().Contain("User ID cannot be empty");
        _mockUserRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task UpdateUserProfileAsync_WithInvalidDisplayName_ShouldThrowException(string displayName)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.UpdateUserProfileAsync(userId, displayName));
        
        exception.Message.Should().Contain("Display name is required");
        _mockUserRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task FollowUserAsync_WithEmptyFollowerId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.FollowUserAsync(Guid.Empty, Guid.NewGuid()));
        
        exception.Message.Should().Contain("Follower ID cannot be empty");
    }

    [Fact]
    public async Task FollowUserAsync_WithEmptyFolloweeId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.FollowUserAsync(Guid.NewGuid(), Guid.Empty));
        
        exception.Message.Should().Contain("Followee ID cannot be empty");
    }

    [Fact]
    public async Task UnfollowUserAsync_WithEmptyFollowerId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.UnfollowUserAsync(Guid.Empty, Guid.NewGuid()));
        
        exception.Message.Should().Contain("Follower ID cannot be empty");
    }

    [Fact]
    public async Task UnfollowUserAsync_WithEmptyFolloweeId_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.UnfollowUserAsync(Guid.NewGuid(), Guid.Empty));
        
        exception.Message.Should().Contain("Followee ID cannot be empty");
    }

    [Fact]
    public async Task SearchUsersAsync_WithZeroLimit_ShouldUseDefaultLimit()
    {
        // Arrange
        var searchTerm = "test";
        var expectedUsers = new List<User> { new User("test@example.com", "test", "Test", "hashedpassword") };
        
        _mockUserRepository.Setup(r => r.SearchByUsernameAsync(searchTerm, 20))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.SearchUsersAsync(searchTerm, 0);

        // Assert
        result.Should().BeEquivalentTo(expectedUsers);
        _mockUserRepository.Verify(r => r.SearchByUsernameAsync(searchTerm, 20), Times.Once);
    }

    [Fact]
    public async Task SearchUsersAsync_WithNegativeLimit_ShouldUseDefaultLimit()
    {
        // Arrange
        var searchTerm = "test";
        var expectedUsers = new List<User> { new User("test@example.com", "test", "Test", "hashedpassword") };
        
        _mockUserRepository.Setup(r => r.SearchByUsernameAsync(searchTerm, 20))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.SearchUsersAsync(searchTerm, -5);

        // Assert
        result.Should().BeEquivalentTo(expectedUsers);
        _mockUserRepository.Verify(r => r.SearchByUsernameAsync(searchTerm, 20), Times.Once);
    }
}
