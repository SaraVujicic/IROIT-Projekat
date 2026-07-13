using System.Net.Http.Json;

namespace RequestService.Application.Services;

public class EmployeeServiceClient : IEmployeeServiceClient
{
    private readonly HttpClient _httpClient;

    public EmployeeServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ValidateEmployeeExistsAsync(int employeeId, CancellationToken cancellationToken)
    {
        // Calling EmployeeService over Docker internal network matching the defined mapping
        var response = await _httpClient.GetAsync($"/api/employees/{employeeId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
