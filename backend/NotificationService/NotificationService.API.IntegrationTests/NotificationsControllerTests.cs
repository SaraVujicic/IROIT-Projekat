using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NotificationService.Application.DTOs;
using Xunit;

namespace NotificationService.API.IntegrationTests
{
    public class NotificationsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public NotificationsControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateNotification_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new CreateNotificationRequest
            {
                RecipientId = 1,
                Message = "Your request was approved."
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/notifications", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var notification = await response.Content.ReadFromJsonAsync<NotificationDto>();
            notification.Should().NotBeNull();
            notification!.RecipientId.Should().Be(1);
            notification.Message.Should().Be("Your request was approved.");
            notification.ReadStatus.Should().BeFalse();
        }

        [Fact]
        public async Task CreateNotification_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateNotificationRequest
            {
                RecipientId = 0, // Invalid
                Message = "Message here"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/notifications", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUserNotifications_ShouldReturnList()
        {
            // Arrange
            var request1 = new CreateNotificationRequest { RecipientId = 55, Message = "Message 1" };
            var request2 = new CreateNotificationRequest { RecipientId = 55, Message = "Message 2" };
            await _client.PostAsJsonAsync("/api/notifications", request1);
            await _client.PostAsJsonAsync("/api/notifications", request2);

            // Act
            var response = await _client.GetAsync("/api/notifications/user/55");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<NotificationDto[]>();
            list.Should().NotBeNull();
            list.Should().HaveCountGreaterThanOrEqualTo(2);
        }
    }
}
