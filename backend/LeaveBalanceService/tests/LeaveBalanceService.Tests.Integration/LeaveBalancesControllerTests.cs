using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveBalanceService.Application.DTOs;
using LeaveBalanceService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Linq; // For SingleOrDefault extension method
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeaveBalanceService.Tests.Integration;

public class LeaveBalancesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LeaveBalancesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's LeaveBalanceDbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<LeaveBalanceDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using an in-memory database for testing.
                services.AddDbContext<LeaveBalanceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting"); // using Microsoft.EntityFrameworkCore.InMemory
                });
            });
        });
    }

    [Fact]
    public async Task CreateLeaveBalance_ValidDto_ReturnsCreated()
    {
        var client = _factory.CreateClient();
        var dto = new CreateLeaveBalanceDto
        {
            // use unique employee id to prevent state leakage from memory db
            EmployeeId = 1, 
            Year = 2024,
            TotalDays = 20,
            CarriedOverDays = 5,
            ExpirationDate = new DateTime(2025, 6, 30)
        };

        var response = await client.PostAsJsonAsync("/api/leavebalances", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<LeaveBalanceResponseDto>();
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be(1);
        result.RemainingDays.Should().Be(25);
    }
}
