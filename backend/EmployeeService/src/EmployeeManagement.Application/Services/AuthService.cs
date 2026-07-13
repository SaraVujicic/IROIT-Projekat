using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Interfaces;

namespace EmployeeManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IEmployeeRepository _repository;

    public AuthService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuthResponseDTO?> LoginAsync(LoginDTO dto)
    {
        var employee = await _repository.GetByUsernameAsync(dto.Username);
        
        if (employee == null)
            return null;

        // Verify the provided password with the stored hash
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, employee.Credentials.PasswordHash);
        if (!isPasswordValid)
            return null;

        var role = employee switch
        {
            Admin => "Admin",
            Manager => "Manager",
            _ => "Employee"
        };

        return new AuthResponseDTO(
            Token: "mock-jwt-token-for-testing",
            Id: employee.Id,
            Role: role,
            FirstName: employee.FirstName,
            LastName: employee.LastName
        );
    }
}
