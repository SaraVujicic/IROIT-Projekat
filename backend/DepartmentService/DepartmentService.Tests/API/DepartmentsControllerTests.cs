namespace DepartmentService.Tests.API;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using DepartmentService.Application.DTOs;
using FluentAssertions;

public class DepartmentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DepartmentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateDepartment_ShouldReturnCreated_WhenValid()
    {
        var client = _factory.CreateClient();
        var dto = new CreateDepartmentDto { Name = "TestDeptIntegration", Description = "Test" };
        
        var response = await client.PostAsJsonAsync("/api/departments", dto);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
