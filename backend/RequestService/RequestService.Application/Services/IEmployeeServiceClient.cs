namespace RequestService.Application.Services;

public interface IEmployeeServiceClient
{
    Task<bool> ValidateEmployeeExistsAsync(int employeeId, CancellationToken cancellationToken);
}
