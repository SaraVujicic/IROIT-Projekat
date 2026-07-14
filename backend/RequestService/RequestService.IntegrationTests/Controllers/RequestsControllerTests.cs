using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RequestService.Application.Commands.CreateLeaveRequest;
using RequestService.Domain.Enums;
using Xunit;
using RequestService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RequestService.IntegrationTests.Controllers;

public class RequestsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RequestsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RequestDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<RequestDbContext>(options =>
                {
                    options.UseInMemoryDatabase("RequestTestDb");
                });

                // Mock IEmployeeServiceClient to return true (employee exists) without calling external service
                var clientDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(RequestService.Application.Services.IEmployeeServiceClient));
                if (clientDescriptor != null)
                {
                    services.Remove(clientDescriptor);
                }
                services.AddSingleton<RequestService.Application.Services.IEmployeeServiceClient, TestEmployeeServiceClient>();

                // Mock IMessageBus to prevent connection to RabbitMQ
                var busDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(RequestService.Application.Interfaces.IMessageBus));
                if (busDescriptor != null)
                {
                    services.Remove(busDescriptor);
                }
                services.AddSingleton<RequestService.Application.Interfaces.IMessageBus, TestMessageBus>();
            });
        }).CreateClient();
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

public class TestEmployeeServiceClient : RequestService.Application.Services.IEmployeeServiceClient
{
    public Task<bool> ValidateEmployeeExistsAsync(int employeeId, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}

public class TestMessageBus : RequestService.Application.Interfaces.IMessageBus
{
    public Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
