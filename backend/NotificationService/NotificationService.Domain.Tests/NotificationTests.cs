using System;
using FluentAssertions;
using NotificationService.Domain.Aggregates.NotificationAggregate;
using NotificationService.Domain.Exceptions;
using Xunit;

namespace NotificationService.Domain.Tests
{
    public class NotificationTests
    {
        [Fact]
        public void CreateNotification_WithValidInputs_ShouldCreateNotification()
        {
            // Arrange
            var recipientId = 123;
            var message = "You have a new message.";

            // Act
            var notification = Notification.CreateNotification(recipientId, message);

            // Assert
            notification.Should().NotBeNull();
            notification.RecipientId.Should().Be(recipientId);
            notification.Message.Should().Be(message);
            notification.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            notification.ReadStatus.Should().BeFalse();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void CreateNotification_WithInvalidRecipientId_ShouldThrowDomainException(int invalidRecipientId)
        {
            // Act
            Action act = () => Notification.CreateNotification(invalidRecipientId, "Valid message");

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("A valid RecipientId must be provided. (Parameter 'recipientId')");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void CreateNotification_WithInvalidMessage_ShouldThrowDomainException(string invalidMessage)
        {
            // Act
            Action act = () => Notification.CreateNotification(123, invalidMessage);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Notification message cannot be empty. (Parameter 'message')");
        }

        [Fact]
        public void MarkAsRead_WhenUnread_ShouldUpdateReadStatus()
        {
            // Arrange
            var notification = Notification.CreateNotification(123, "Message");

            // Act
            notification.MarkAsRead();

            // Assert
            notification.ReadStatus.Should().BeTrue();
        }

        [Fact]
        public void MarkAsRead_WhenAlreadyRead_ShouldThrowDomainException()
        {
            // Arrange
            var notification = Notification.CreateNotification(123, "Message");
            notification.MarkAsRead();

            // Act
            Action act = () => notification.MarkAsRead();

            // Assert
            act.Should().Throw<DomainException>()
               .WithMessage("Notification is already marked as read.");
        }

        [Fact]
        public void MarkAsUnread_WhenRead_ShouldUpdateReadStatusToFalse()
        {
            // Arrange
            var notification = Notification.CreateNotification(123, "Message");
            notification.MarkAsRead();

            // Act
            notification.MarkAsUnread();

            // Assert
            notification.ReadStatus.Should().BeFalse();
        }

        [Fact]
        public void MarkAsUnread_WhenAlreadyUnread_ShouldThrowDomainException()
        {
            // Arrange
            var notification = Notification.CreateNotification(123, "Message");

            // Act
            Action act = () => notification.MarkAsUnread();

            // Assert
            act.Should().Throw<DomainException>()
               .WithMessage("Notification is already unread.");
        }
    }
}
