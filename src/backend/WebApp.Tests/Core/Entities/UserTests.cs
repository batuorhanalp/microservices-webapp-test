using FluentAssertions;
using WebApp.Core.Entities;
using Xunit;

namespace WebApp.Tests.Core.Entities;

public class UserTests
{
    [Fact]
    public void CreateUser_WithValidData_ShouldCreateUserWithCorrectProperties()
    {
        // Arrange
        var email = "test@example.com";
        var username = "testuser";
        var displayName = "Test User";

        // Act
        var user = new User(email, username, displayName);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be(email.ToLowerInvariant());
        user.Username.Should().Be(username.ToLowerInvariant());
        user.DisplayName.Should().Be(displayName);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.IsPrivate.Should().BeFalse();
        user.IsVerified.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateUser_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Arrange & Act & Assert
        var action = () => new User(invalidEmail, "username", "Display Name");
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email is required*")
            .And.ParamName.Should().Be("email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateUser_WithInvalidUsername_ShouldThrowArgumentException(string invalidUsername)
    {
        // Arrange & Act & Assert
        var action = () => new User("test@example.com", invalidUsername, "Display Name");
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Username is required*")
            .And.ParamName.Should().Be("username");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateUser_WithInvalidDisplayName_ShouldThrowArgumentException(string invalidDisplayName)
    {
        // Arrange & Act & Assert
        var action = () => new User("test@example.com", "username", invalidDisplayName);
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("Display name is required*")
            .And.ParamName.Should().Be("displayName");
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateFields()
    {
        // Arrange
        var user = new User("test@example.com", "username", "Original Name");
        var originalUpdatedAt = user.UpdatedAt;
        
        // Act
        user.UpdateProfile("New Display Name", "New bio", "https://example.com", "New Location");

        // Assert
        user.DisplayName.Should().Be("New Display Name");
        user.Bio.Should().Be("New bio");
        user.Website.Should().Be("https://example.com");
        user.Location.Should().Be("New Location");
        user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateProfile_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var user = new User("test@example.com", "username", "Original Name");

        // Act
        user.UpdateProfile("New Name", null, null, null);

        // Assert
        user.DisplayName.Should().Be("New Name");
        user.Bio.Should().Be(string.Empty);
        user.Website.Should().Be(string.Empty);
        user.Location.Should().Be(string.Empty);
    }

    [Fact]
    public void SetPrivacyStatus_WhenCalled_ShouldUpdatePrivacyAndTimestamp()
    {
        // Arrange
        var user = new User("test@example.com", "username", "Display Name");
        var originalUpdatedAt = user.UpdatedAt;
        Thread.Sleep(1); // Ensure time difference

        // Act
        user.SetPrivacyStatus(true);

        // Assert
        user.IsPrivate.Should().BeTrue();
        user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void SetVerificationStatus_WhenCalled_ShouldUpdateVerificationAndTimestamp()
    {
        // Arrange
        var user = new User("test@example.com", "username", "Display Name");
        var originalUpdatedAt = user.UpdatedAt;
        Thread.Sleep(1); // Ensure time difference

        // Act
        user.SetVerificationStatus(true);

        // Assert
        user.IsVerified.Should().BeTrue();
        user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void SetBirthDate_WithValidAge_ShouldSetBirthDate()
    {
        // Arrange
        var user = new User("test@example.com", "username", "Display Name");
        var validBirthDate = DateTime.UtcNow.AddYears(-20);

        // Act
        user.SetBirthDate(validBirthDate);

        // Assert
        user.BirthDate.Should().Be(validBirthDate);
    }

    [Fact]
    public void SetBirthDate_WithUnderageUser_ShouldThrowException()
    {
        // Arrange
        var user = new User("test@example.com", "username", "Display Name");
        var underageBirthDate = DateTime.UtcNow.AddYears(-10);

        // Act & Assert
        var action = () => user.SetBirthDate(underageBirthDate);
        
        action.Should().Throw<ArgumentException>()
            .WithMessage("User must be at least 13 years old");
    }

    [Fact]
    public void UpdateProfileImage_WithValidUrl_ShouldUpdateImageAndTimestamp()
    {
        // Arrange
        var user = new User("test@example.com", "username", "Display Name");
        var imageUrl = "https://example.com/image.jpg";
        var originalUpdatedAt = user.UpdatedAt;
        Thread.Sleep(1);

        // Act
        user.UpdateProfileImage(imageUrl);

        // Assert
        user.ProfileImageUrl.Should().Be(imageUrl);
        user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateCoverImage_WithValidUrl_ShouldUpdateImageAndTimestamp()
    {
        // Arrange
        var user = new User("test@example.com", "username", "Display Name");
        var coverUrl = "https://example.com/cover.jpg";
        var originalUpdatedAt = user.UpdatedAt;
        Thread.Sleep(1);

        // Act
        user.UpdateCoverImage(coverUrl);

        // Assert
        user.CoverImageUrl.Should().Be(coverUrl);
        user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
