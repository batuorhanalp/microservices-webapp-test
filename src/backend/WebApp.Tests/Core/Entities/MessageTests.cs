using FluentAssertions;
using WebApp.Common.Entities;
using Xunit;

namespace WebApp.Tests.Core.Entities;

public class MessageTests
{
    [Fact]
    public void CreateMessage_WithValidTextData_ShouldCreateMessageWithCorrectProperties()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var content = "Hello there!";

        // Act
        var message = new Message(senderId, recipientId, content);

        // Assert
        message.Id.Should().NotBeEmpty();
        message.SenderId.Should().Be(senderId);
        message.RecipientId.Should().Be(recipientId);
        message.Content.Should().Be(content);
        message.Type.Should().Be(MessageType.Text);
        message.IsRead.Should().BeFalse();
        message.IsEdited.Should().BeFalse();
        message.IsDeleted.Should().BeFalse();
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        message.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        message.ReadAt.Should().BeNull();
        message.AttachmentUrl.Should().BeNull();
        message.AttachmentFileName.Should().BeNull();
        message.AttachmentFileSize.Should().BeNull();
        message.HasAttachment.Should().BeFalse();
    }

    [Fact]
    public void CreateMessage_WithEmptySenderId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Message(Guid.Empty, Guid.NewGuid(), "Content");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Sender ID is required*")
            .And.ParamName.Should().Be("senderId");
    }

    [Fact]
    public void CreateMessage_WithEmptyRecipientId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Message(Guid.NewGuid(), Guid.Empty, "Content");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipient ID is required*")
            .And.ParamName.Should().Be("recipientId");
    }

    [Fact]
    public void CreateMessage_WithSameSenderAndRecipient_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var action = () => new Message(userId, userId, "Content");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Sender and recipient cannot be the same");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateMessage_WithEmptyContentForTextMessage_ShouldThrowArgumentException(string invalidContent)
    {
        // Act & Assert
        var action = () => new Message(Guid.NewGuid(), Guid.NewGuid(), invalidContent, MessageType.Text);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Content is required for text messages*")
            .And.ParamName.Should().Be("content");
    }

    [Fact]
    public void CreateMessage_WithEmptyContentForNonTextMessage_ShouldNotThrowException()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        // Act
        var message = new Message(senderId, recipientId, "", MessageType.Image);

        // Assert
        message.Content.Should().Be("");
        message.Type.Should().Be(MessageType.Image);
    }

    [Fact]
    public void UpdateContent_WithValidContent_ShouldUpdateContentAndMarkAsEdited()
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Original content");
        var newContent = "Updated content";
        var originalUpdatedAt = message.UpdatedAt;
        Thread.Sleep(1);

        // Act
        message.UpdateContent(newContent);

        // Assert
        message.Content.Should().Be(newContent);
        message.IsEdited.Should().BeTrue();
        message.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateContent_WhenMessageIsDeleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Original content");
        message.Delete();

        // Act & Assert
        var action = () => message.UpdateContent("New content");

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot edit deleted message");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateContent_WithEmptyContentForTextMessage_ShouldThrowArgumentException(string invalidContent)
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Original content", MessageType.Text);

        // Act & Assert
        var action = () => message.UpdateContent(invalidContent);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be empty for text messages*")
            .And.ParamName.Should().Be("content");
    }

    [Fact]
    public void MarkAsRead_WhenUnread_ShouldMarkAsReadWithTimestamp()
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Content");

        // Act
        message.MarkAsRead();

        // Assert
        message.IsRead.Should().BeTrue();
        message.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_ShouldNotChangeReadAt()
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Content");
        message.MarkAsRead();
        var originalReadAt = message.ReadAt;
        Thread.Sleep(1);

        // Act
        message.MarkAsRead();

        // Assert
        message.IsRead.Should().BeTrue();
        message.ReadAt.Should().Be(originalReadAt);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeletedAndUpdateTimestamp()
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Content");
        var originalUpdatedAt = message.UpdatedAt;
        Thread.Sleep(1);

        // Act
        message.Delete();

        // Assert
        message.IsDeleted.Should().BeTrue();
        message.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void SetAttachment_WithValidData_ShouldSetAttachmentPropertiesAndUpdateTimestamp()
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Content");
        var url = "https://example.com/file.jpg";
        var fileName = "file.jpg";
        var fileSize = 1024L;
        var originalUpdatedAt = message.UpdatedAt;
        Thread.Sleep(1);

        // Act
        message.SetAttachment(url, fileName, fileSize);

        // Assert
        message.AttachmentUrl.Should().Be(url);
        message.AttachmentFileName.Should().Be(fileName);
        message.AttachmentFileSize.Should().Be(fileSize);
        message.HasAttachment.Should().BeTrue();
        message.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void SetAttachment_WithInvalidUrl_ShouldThrowArgumentException(string invalidUrl)
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Content");

        // Act & Assert
        var action = () => message.SetAttachment(invalidUrl, "file.jpg", 1024L);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Attachment URL is required*")
            .And.ParamName.Should().Be("url");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void SetAttachment_WithInvalidFileName_ShouldThrowArgumentException(string invalidFileName)
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Content");

        // Act & Assert
        var action = () => message.SetAttachment("https://example.com/file.jpg", invalidFileName, 1024L);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Attachment file name is required*")
            .And.ParamName.Should().Be("fileName");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void SetAttachment_WithInvalidFileSize_ShouldThrowArgumentException(long invalidFileSize)
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Content");

        // Act & Assert
        var action = () => message.SetAttachment("https://example.com/file.jpg", "file.jpg", invalidFileSize);

        action.Should().Throw<ArgumentException>()
            .WithMessage("File size must be positive*")
            .And.ParamName.Should().Be("fileSize");
    }

    [Fact]
    public void HasAttachment_WithoutAttachment_ShouldReturnFalse()
    {
        // Arrange
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), "Content");

        // Act & Assert
        message.HasAttachment.Should().BeFalse();
    }

    [Theory]
    [InlineData(MessageType.Text)]
    [InlineData(MessageType.Image)]
    [InlineData(MessageType.Video)]
    [InlineData(MessageType.Audio)]
    [InlineData(MessageType.File)]
    public void CreateMessage_WithDifferentTypes_ShouldSetTypeCorrectly(MessageType messageType)
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var content = messageType == MessageType.Text ? "Text content" : "";

        // Act
        var message = new Message(senderId, recipientId, content, messageType);

        // Assert
        message.Type.Should().Be(messageType);
    }
}
