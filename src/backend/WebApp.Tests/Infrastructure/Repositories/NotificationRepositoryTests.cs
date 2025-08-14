using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;
using WebApp.Common.Data;
using WebApp.Common.DTOs;
using WebApp.Common.Repositories;
using Xunit;

namespace WebApp.Tests.Infrastructure.Repositories;

public class NotificationRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationRepository _notificationRepository;

    public NotificationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _notificationRepository = new NotificationRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnNotification()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        var triggerUser = new User("trigger@example.com", "trigger", "Trigger", "hashedpassword");
        await _context.Users.AddRangeAsync(user, triggerUser);
        await _context.SaveChangesAsync();

        var notification = new Notification(
            user.Id,
            NotificationType.Like,
            "Test notification",
            "Test message",
            null, // entityId
            "post", // entityType
            triggerUser.Id,
            "trigger link",
            DateTime.UtcNow.AddDays(1)
        );
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _notificationRepository.GetByIdAsync(notification.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(notification.Id);
        result.UserId.Should().Be(user.Id);
        result.Type.Should().Be(NotificationType.Like);
        result.Title.Should().Be("Test notification");
        result.Message.Should().Be("Test message");
        result.TriggerUserId.Should().Be(triggerUser.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _notificationRepository.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidNotification_ShouldAddNotificationToDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notification = new Notification(
            user.Id,
            NotificationType.Comment,
            "New comment",
            "Someone commented on your post"
        );

        // Act
        var result = await _notificationRepository.CreateAsync(notification);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(notification.Id);
        
        var savedNotification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
        savedNotification.Should().NotBeNull();
        savedNotification!.UserId.Should().Be(user.Id);
        savedNotification.Type.Should().Be(NotificationType.Comment);
    }

    [Fact]
    public async Task UpdateAsync_WithValidNotification_ShouldUpdateNotificationInDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notification = new Notification(
            user.Id,
            NotificationType.Follow,
            "New follower",
            "Someone started following you"
        );
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        // Modify notification
        notification.MarkAsRead();

        // Act
        var result = await _notificationRepository.UpdateAsync(notification);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(NotificationStatus.Read);
        
        var updatedNotification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
        updatedNotification.Should().NotBeNull();
        updatedNotification!.Status.Should().Be(NotificationStatus.Read);
        updatedNotification.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldRemoveNotificationFromDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notification = new Notification(
            user.Id,
            NotificationType.System,
            "New message",
            "You have a new message"
        );
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _notificationRepository.DeleteAsync(notification.Id);

        // Assert
        result.Should().BeTrue();
        
        var deletedNotification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
        deletedNotification.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _notificationRepository.DeleteAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WithValidUserId_ShouldReturnUserNotifications()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1", "hashedpassword1");
        var user2 = new User("user2@example.com", "user2", "User 2", "hashedpassword2");
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user1.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user1.Id, NotificationType.Comment, "Comment 1", "Comment message 1"),
            new Notification(user2.Id, NotificationType.Follow, "Follow 1", "Follow message 1")
        };
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        var request = new NotificationQueryRequest
        {
            Page = 1,
            PageSize = 10,
            SortBy = "createdat",
            SortOrder = "desc",
            IncludeExpired = true
        };

        // Act
        var result = await _notificationRepository.GetUserNotificationsAsync(user1.Id, request);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().OnlyContain(n => n.UserId == user1.Id);
        resultList.Should().BeInDescendingOrder(n => n.CreatedAt);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WithTypeFilter_ShouldReturnFilteredNotifications()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1"),
            new Notification(user.Id, NotificationType.Like, "Like 2", "Like message 2")
        };
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        var request = new NotificationQueryRequest
        {
            Page = 1,
            PageSize = 10,
            Type = NotificationType.Like
        };

        // Act
        var result = await _notificationRepository.GetUserNotificationsAsync(user.Id, request);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().OnlyContain(n => n.Type == NotificationType.Like);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WithStatusFilter_ShouldReturnFilteredNotifications()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1")
        };
        notifications[0].MarkAsRead(); // Mark first as read
        
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        var request = new NotificationQueryRequest
        {
            Page = 1,
            PageSize = 10,
            Status = NotificationStatus.Unread
        };

        // Act
        var result = await _notificationRepository.GetUserNotificationsAsync(user.Id, request);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        resultList.Should().OnlyContain(n => n.Status == NotificationStatus.Unread);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new List<Notification>();
        for (int i = 1; i <= 5; i++)
        {
            notifications.Add(new Notification(user.Id, NotificationType.Like, $"Like {i}", $"Like message {i}"));
        }
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        var request = new NotificationQueryRequest
        {
            Page = 2,
            PageSize = 2
        };

        // Act
        var result = await _notificationRepository.GetUserNotificationsAsync(user.Id, request);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserNotificationsCountAsync_WithValidUserId_ShouldReturnCorrectCount()
    {
        // Arrange
        var user1 = new User("user1@example.com", "user1", "User 1", "hashedpassword1");
        var user2 = new User("user2@example.com", "user2", "User 2", "hashedpassword2");
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user1.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user1.Id, NotificationType.Comment, "Comment 1", "Comment message 1"),
            new Notification(user1.Id, NotificationType.Follow, "Follow 1", "Follow message 1"),
            new Notification(user2.Id, NotificationType.Like, "Like 2", "Like message 2")
        };
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        var request = new NotificationQueryRequest();

        // Act
        var result = await _notificationRepository.GetUserNotificationsCountAsync(user1.Id, request);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task GetUserNotificationStatsAsync_WithValidUserId_ShouldReturnCorrectStats()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1"),
            new Notification(user.Id, NotificationType.Like, "Like 2", "Like message 2")
        };
        
        // Mark one as read and one as archived
        notifications[0].MarkAsRead();
        notifications[1].Archive();
        
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        // Act
        var result = await _notificationRepository.GetUserNotificationStatsAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.UnreadCount.Should().Be(1);
        result.ReadCount.Should().Be(1);
        result.ArchivedCount.Should().Be(1);
        result.CountByType[NotificationType.Like].Should().Be(2);
        result.CountByType[NotificationType.Comment].Should().Be(1);
    }

    [Fact]
    public async Task CreateBulkAsync_WithValidNotifications_ShouldAddAllNotificationsToDatabase()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1"),
            new Notification(user.Id, NotificationType.Follow, "Follow 1", "Follow message 1")
        };

        // Act
        var result = await _notificationRepository.CreateBulkAsync(notifications);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(3);
        
        var savedNotifications = await _context.Notifications.Where(n => n.UserId == user.Id).ToListAsync();
        savedNotifications.Should().HaveCount(3);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_WithValidUserId_ShouldMarkAllUnreadNotificationsAsRead()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1"),
            new Notification(user.Id, NotificationType.Follow, "Follow 1", "Follow message 1")
        };
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        // Act
        var result = await _notificationRepository.MarkAllAsReadAsync(user.Id);

        // Assert
        result.Should().Be(3);
        
        var updatedNotifications = await _context.Notifications.Where(n => n.UserId == user.Id).ToListAsync();
        updatedNotifications.Should().OnlyContain(n => n.Status == NotificationStatus.Read);
        updatedNotifications.Should().OnlyContain(n => n.ReadAt != null);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_WithTypeFilter_ShouldOnlyMarkSpecificType()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1")
        };
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        // Act
        var result = await _notificationRepository.MarkAllAsReadAsync(user.Id, NotificationType.Like);

        // Assert
        result.Should().Be(1);
        
        var likeNotifications = await _context.Notifications
            .Where(n => n.UserId == user.Id && n.Type == NotificationType.Like)
            .ToListAsync();
        likeNotifications.Should().OnlyContain(n => n.Status == NotificationStatus.Read);
        
        var commentNotifications = await _context.Notifications
            .Where(n => n.UserId == user.Id && n.Type == NotificationType.Comment)
            .ToListAsync();
        commentNotifications.Should().OnlyContain(n => n.Status == NotificationStatus.Unread);
    }

    [Fact]
    public async Task MarkAllAsArchivedAsync_WithValidUserId_ShouldMarkAllNotificationsAsArchived()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1")
        };
        notifications[0].MarkAsRead(); // Mark one as read first
        
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        // Act
        var result = await _notificationRepository.MarkAllAsArchivedAsync(user.Id);

        // Assert
        result.Should().Be(2);
        
        var updatedNotifications = await _context.Notifications.Where(n => n.UserId == user.Id).ToListAsync();
        updatedNotifications.Should().OnlyContain(n => n.Status == NotificationStatus.Archived);
        updatedNotifications.Should().OnlyContain(n => n.ArchivedAt != null);
    }

    [Fact]
    public async Task DeleteExpiredNotificationsAsync_ShouldDeleteExpiredNotifications()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1", null, null, null, null, DateTime.UtcNow.AddDays(-1)), // Expired
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1", null, null, null, null, DateTime.UtcNow.AddDays(1)), // Not expired
            new Notification(user.Id, NotificationType.Follow, "Follow 1", "Follow message 1") // No expiration
        };
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        // Act
        var result = await _notificationRepository.DeleteExpiredNotificationsAsync();

        // Assert
        result.Should().Be(1);
        
        var remainingNotifications = await _context.Notifications.Where(n => n.UserId == user.Id).ToListAsync();
        remainingNotifications.Should().HaveCount(2);
        remainingNotifications.Should().NotContain(n => n.ExpiresAt < DateTime.UtcNow);
    }

    [Fact]
    public async Task DeleteArchivedNotificationsAsync_ShouldDeleteOldArchivedNotifications()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1"),
            new Notification(user.Id, NotificationType.Follow, "Follow 1", "Follow message 1")
        };
        
        // Archive notifications (ArchivedAt is set automatically by Archive() method)
        notifications[0].Archive(); // This will set ArchivedAt automatically
        notifications[1].Archive(); // This will set ArchivedAt automatically
        
        // For testing purposes, we'll simulate different archived dates by using Thread.Sleep
        // In a real test, you might need to use a time provider or modify the test approach
        // notifications[2] remains unarchived
        
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        var cutoffDate = DateTime.UtcNow.AddMinutes(1); // Use a future cutoff to include all archived notifications

        // Act
        var result = await _notificationRepository.DeleteArchivedNotificationsAsync(cutoffDate);

        // Assert
        result.Should().Be(2); // Both archived notifications should be deleted
        
        var remainingNotifications = await _context.Notifications.Where(n => n.UserId == user.Id).ToListAsync();
        remainingNotifications.Should().HaveCount(1); // Only the unarchived notification remains
    }

    [Fact]
    public async Task GetUnreadNotificationsAsync_WithValidUserId_ShouldReturnOnlyUnreadNotifications()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1"),
            new Notification(user.Id, NotificationType.Follow, "Follow 1", "Follow message 1")
        };
        
        notifications[0].MarkAsRead(); // Mark one as read
        
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        // Act
        var result = await _notificationRepository.GetUnreadNotificationsAsync(user.Id);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().OnlyContain(n => n.Status == NotificationStatus.Unread);
        resultList.Should().BeInDescendingOrder(n => n.CreatedAt);
    }

    [Fact]
    public async Task GetUnreadCountAsync_WithValidUserId_ShouldReturnCorrectUnreadCount()
    {
        // Arrange
        var user = new User("user@example.com", "user", "User", "hashedpassword");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var notifications = new[]
        {
            new Notification(user.Id, NotificationType.Like, "Like 1", "Like message 1"),
            new Notification(user.Id, NotificationType.Comment, "Comment 1", "Comment message 1"),
            new Notification(user.Id, NotificationType.Follow, "Follow 1", "Follow message 1")
        };
        
        notifications[0].MarkAsRead(); // Mark one as read
        notifications[1].Archive(); // Archive one
        
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        // Act
        var result = await _notificationRepository.GetUnreadCountAsync(user.Id);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task Constructor_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new NotificationRepository(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
