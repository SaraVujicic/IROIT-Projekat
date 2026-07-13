using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RequestService.Application.Commands.CreateLeaveRequest;
using RequestService.Domain.Enums;
using Xunit;

namespace RequestService.IntegrationTests.Controllers;

public class RequestsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RequestsControllerTests(WebApplicationFactory<Program> factory)
    {
        // Uses InMemory DB configured in DI since connection string is omitted
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateLeaveRequest_ReturnsOk_WhenValid()
    {
        // Arrange
        var command = new CreateLeaveRequestCommand(
            1, 
            "Test Leave", 
            DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(3), 
            LeaveType.SickLeave);

        // Act
        var response = await _client.PostAsJsonAsync("/api/requests/leave", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var id = await response.Content.ReadFromJsonAsync<int>();
        Assert.True(id > 0);
    }
}
