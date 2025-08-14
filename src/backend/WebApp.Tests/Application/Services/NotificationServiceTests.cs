using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Common.DTOs;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
using WebApp.Common.Services;
using Xunit;

namespace WebApp.Tests.Application.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _mockNotificationRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly INotificationService _notificationService;

    public NotificationServiceTests()
    {
        _mockNotificationRepository = new Mock<INotificationRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _notificationService = new NotificationService(
            _mockNotificationRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetNotificationAsync_WithValidId_ShouldReturnNotificationDto()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = new Notification(
            Guid.NewGuid(),
            NotificationType.Like,
            "Test notification",
            "Test message");

        _mockNotificationRepository
            .Setup(r => r.GetByIdAsync(notificationId, default))
            .ReturnsAsync(notification);

        // Act
        var result = await _notificationService.GetNotificationAsync(notificationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(notification.Id);
        result.Type.Should().Be(NotificationType.Like);
        result.Title.Should().Be("Test notification");
        result.Message.Should().Be("Test message");
    }

    [Fact]
    public async Task GetNotificationAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _mockNotificationRepository
            .Setup(r => r.GetByIdAsync(notificationId, default))
            .ReturnsAsync((Notification?)null);

        // Act
        var result = await _notificationService.GetNotificationAsync(notificationId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetNotificationAsync_WhenRepositoryThrows_ShouldRethrowException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Database error");
        
        _mockNotificationRepository
            .Setup(r => r.GetByIdAsync(notificationId, default))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _notificationService.GetNotificationAsync(notificationId));
        
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithValidRequest_ShouldCreateAndReturnNotificationDto()
    {
        // Arrange
        var request = new CreateNotificationRequest
        {
            UserId = Guid.NewGuid(),
            Type = NotificationType.Comment,
            Title = "New comment",
            Message = "Someone commented on your post",
            EntityId = Guid.NewGuid(),
            EntityType = "Post",
            TriggerUserId = Guid.NewGuid(),
            ActionUrl = "/posts/123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Metadata = new Dictionary<string, object>
            {
                { "postId", "123" },
                { "commentId", "456" }
            }
        };

        var createdNotification = new Notification(
            request.UserId,
            request.Type,
            request.Title,
            request.Message,
            request.EntityId,
            request.EntityType,
            request.TriggerUserId,
            request.ActionUrl,
            request.ExpiresAt);

        _mockNotificationRepository
            .Setup(r => r.CreateAsync(It.IsAny<Notification>(), default))
            .ReturnsAsync(createdNotification);

        // Act
        var result = await _notificationService.CreateNotificationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(request.UserId);
        result.Type.Should().Be(request.Type);
        result.Title.Should().Be(request.Title);
        result.Message.Should().Be(request.Message);
        result.EntityId.Should().Be(request.EntityId);
        result.EntityType.Should().Be(request.EntityType);
        result.TriggerUserId.Should().Be(request.TriggerUserId);
        result.ActionUrl.Should().Be(request.ActionUrl);

        _mockNotificationRepository.Verify(
            r => r.CreateAsync(It.IsAny<Notification>(), default),
            Times.Once);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNullMetadata_ShouldCreateNotificationWithoutMetadata()
    {
        // Arrange
        var request = new CreateNotificationRequest
        {
            UserId = Guid.NewGuid(),
            Type = NotificationType.Follow,
            Title = "New follower",
            Message = "Someone started following you",
            Metadata = null
        };

        var createdNotification = new Notification(
            request.UserId,
            request.Type,
            request.Title,
            request.Message);

        _mockNotificationRepository
            .Setup(r => r.CreateAsync(It.IsAny<Notification>(), default))
            .ReturnsAsync(createdNotification);

        // Act
        var result = await _notificationService.CreateNotificationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(request.UserId);
        result.Title.Should().Be(request.Title);
        result.Message.Should().Be(request.Message);
    }

    [Fact]
    public async Task CreateNotificationAsync_WhenRepositoryThrows_ShouldRethrowException()
    {
        // Arrange
        var request = new CreateNotificationRequest
        {
            UserId = Guid.NewGuid(),
            Type = NotificationType.Like,
            Title = "Test",
            Message = "Test message"
        };

        var expectedException = new InvalidOperationException("Database error");
        _mockNotificationRepository
            .Setup(r => r.CreateAsync(It.IsAny<Notification>(), default))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _notificationService.CreateNotificationAsync(request));
        
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task CreateBulkNotificationAsync_WithValidRequest_ShouldCreateBulkNotifications()
    {
        // Arrange
        var request = new BulkNotificationRequest
        {
            UserIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
            Type = NotificationType.System,
            Title = "System notification",
            Message = "System maintenance scheduled",
            EntityId = Guid.NewGuid(),
            EntityType = "System",
            TriggerUserId = null,
            ActionUrl = "/maintenance",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Metadata = new Dictionary<string, object>
            {
                { "maintenanceType", "scheduled" },
                { "duration", "2 hours" }
            }
        };

        var createdNotifications = request.UserIds.Select(userId => 
            new Notification(
                userId,
                request.Type,
                request.Title,
                request.Message,
                request.EntityId,
                request.EntityType,
                request.TriggerUserId,
                request.ActionUrl,
                request.ExpiresAt)).ToList();

        _mockNotificationRepository
            .Setup(r => r.CreateBulkAsync(It.IsAny<IEnumerable<Notification>>(), default))
            .ReturnsAsync(createdNotifications);

        // Act
        var result = await _notificationService.CreateBulkNotificationAsync(request);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(3);
        resultList.Should().AllSatisfy(dto =>
        {
            dto.Type.Should().Be(request.Type);
            dto.Title.Should().Be(request.Title);
            dto.Message.Should().Be(request.Message);
            dto.EntityId.Should().Be(request.EntityId);
            dto.EntityType.Should().Be(request.EntityType);
            dto.ActionUrl.Should().Be(request.ActionUrl);
        });

        _mockNotificationRepository.Verify(
            r => r.CreateBulkAsync(It.Is<IEnumerable<Notification>>(n => n.Count() == 3), default),
            Times.Once);
    }

    [Fact]
    public async Task CreateBulkNotificationAsync_WithEmptyUserIds_ShouldCreateNoNotifications()
    {
        // Arrange
        var request = new BulkNotificationRequest
        {
            UserIds = new List<Guid>(),
            Type = NotificationType.System,
            Title = "System notification",
            Message = "System maintenance scheduled"
        };

        _mockNotificationRepository
            .Setup(r => r.CreateBulkAsync(It.IsAny<IEnumerable<Notification>>(), default))
            .ReturnsAsync(new List<Notification>());

        // Act
        var result = await _notificationService.CreateBulkNotificationAsync(request);

        // Assert
        result.Should().BeEmpty();
        _mockNotificationRepository.Verify(
            r => r.CreateBulkAsync(It.Is<IEnumerable<Notification>>(n => !n.Any()), default),
            Times.Once);
    }

    [Fact]
    public async Task DeleteNotificationAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _mockNotificationRepository
            .Setup(r => r.DeleteAsync(notificationId, default))
            .ReturnsAsync(true);

        // Act
        var result = await _notificationService.DeleteNotificationAsync(notificationId);

        // Assert
        result.Should().BeTrue();
        _mockNotificationRepository.Verify(
            r => r.DeleteAsync(notificationId, default),
            Times.Once);
    }

    [Fact]
    public async Task DeleteNotificationAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _mockNotificationRepository
            .Setup(r => r.DeleteAsync(notificationId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _notificationService.DeleteNotificationAsync(notificationId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WithValidUserId_ShouldReturnNotificationDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new NotificationQueryRequest
        {
            Page = 1,
            PageSize = 10,
            Type = NotificationType.Like,
            Status = NotificationStatus.Unread
        };

        var notifications = new List<Notification>
        {
            new Notification(userId, NotificationType.Like, "Like 1", "Someone liked your post"),
            new Notification(userId, NotificationType.Like, "Like 2", "Someone else liked your post")
        };

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, request, default))
            .ReturnsAsync(notifications);

        // Act
        var result = await _notificationService.GetUserNotificationsAsync(userId, request);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().AllSatisfy(dto => dto.UserId.Should().Be(userId));
        resultList.Should().AllSatisfy(dto => dto.Type.Should().Be(NotificationType.Like));
    }

    [Fact]
    public async Task GetUserNotificationStatsAsync_WithValidUserId_ShouldReturnStats()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedStats = new NotificationStatsDto
        {
            TotalCount = 10,
            UnreadCount = 5,
            ReadCount = 3,
            ArchivedCount = 2,
            CountByType = new Dictionary<NotificationType, int>
            {
                { NotificationType.Like, 4 },
                { NotificationType.Comment, 3 },
                { NotificationType.Follow, 3 }
            }
        };

        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationStatsAsync(userId, default))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _notificationService.GetUserNotificationStatsAsync(userId);

        // Assert
        result.Should().Be(expectedStats);
        result.TotalCount.Should().Be(10);
        result.UnreadCount.Should().Be(5);
        result.ReadCount.Should().Be(3);
        result.ArchivedCount.Should().Be(2);
        result.CountByType.Should().HaveCount(3);
    }

    [Fact]
    public async Task MarkAsReadAsync_WithValidId_ShouldMarkAndReturnUpdatedDto()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = new Notification(
            Guid.NewGuid(),
            NotificationType.Comment,
            "New comment",
            "Someone commented on your post");

        var updatedNotification = notification;
        updatedNotification.MarkAsRead();

        _mockNotificationRepository
            .Setup(r => r.GetByIdAsync(notificationId, default))
            .ReturnsAsync(notification);
        _mockNotificationRepository
            .Setup(r => r.UpdateAsync(notification, default))
            .ReturnsAsync(updatedNotification);

        // Act
        var result = await _notificationService.MarkAsReadAsync(notificationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(notification.Id);
        result.Status.Should().Be(NotificationStatus.Read);
        
        _mockNotificationRepository.Verify(
            r => r.GetByIdAsync(notificationId, default),
            Times.Once);
        _mockNotificationRepository.Verify(
            r => r.UpdateAsync(notification, default),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _mockNotificationRepository
            .Setup(r => r.GetByIdAsync(notificationId, default))
            .ReturnsAsync((Notification?)null);

        // Act
        var result = await _notificationService.MarkAsReadAsync(notificationId);

        // Assert
        result.Should().BeNull();
        _mockNotificationRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Notification>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Constructor_WithNullNotificationRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new NotificationService(
            null!,
            _mockUserRepository.Object,
            _mockLogger.Object);
        
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new NotificationService(
            _mockNotificationRepository.Object,
            null!,
            _mockLogger.Object);
        
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new NotificationService(
            _mockNotificationRepository.Object,
            _mockUserRepository.Object,
            null!);
        
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WhenRepositoryThrows_ShouldRethrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new NotificationQueryRequest();
        var expectedException = new InvalidOperationException("Database error");
        
        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationsAsync(userId, request, default))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _notificationService.GetUserNotificationsAsync(userId, request));
        
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task GetUserNotificationStatsAsync_WhenRepositoryThrows_ShouldRethrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Database error");
        
        _mockNotificationRepository
            .Setup(r => r.GetUserNotificationStatsAsync(userId, default))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _notificationService.GetUserNotificationStatsAsync(userId));
        
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task DeleteNotificationAsync_WhenRepositoryThrows_ShouldRethrowException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Database error");
        
        _mockNotificationRepository
            .Setup(r => r.DeleteAsync(notificationId, default))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _notificationService.DeleteNotificationAsync(notificationId));
        
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenGetByIdThrows_ShouldRethrowException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Database error");
        
        _mockNotificationRepository
            .Setup(r => r.GetByIdAsync(notificationId, default))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _notificationService.MarkAsReadAsync(notificationId));
        
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task CreateBulkNotificationAsync_WhenRepositoryThrows_ShouldRethrowException()
    {
        // Arrange
        var request = new BulkNotificationRequest
        {
            UserIds = new List<Guid> { Guid.NewGuid() },
            Type = NotificationType.System,
            Title = "Test",
            Message = "Test message"
        };
        
        var expectedException = new InvalidOperationException("Database error");
        _mockNotificationRepository
            .Setup(r => r.CreateBulkAsync(It.IsAny<IEnumerable<Notification>>(), default))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _notificationService.CreateBulkNotificationAsync(request));
        
        exception.Should().Be(expectedException);
    }
}
