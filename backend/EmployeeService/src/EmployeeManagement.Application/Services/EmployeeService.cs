using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmployeeResponseDTO> CreateEmployeeAsync(CreateEmployeeDTO dto)
    {
        // Hash the password
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var credentials = new CredentialsVO(dto.Username, hashedPassword);

        Employee employee;

        switch (dto.Role)
        {
            case "Admin":
                employee = new Admin(dto.FirstName, dto.LastName, credentials, dto.Phone, dto.Department);
                break;
            case "Manager":
                employee = new Manager(dto.FirstName, dto.LastName, credentials, dto.Phone, dto.Department);
                break;
            case "Employee":
                employee = new Employee(dto.FirstName, dto.LastName, credentials, dto.Phone, dto.Department);
                break;
            default:
                throw new DomainException("Invalid role specified.");
        }

        if (dto.ManagerId.HasValue)
        {
            var manager = await _repository.GetByIdAsync(dto.ManagerId.Value);
            if (manager == null || manager is not Manager)
            {
                throw new DomainException("Invalid Manager ID. Assigned user must exist and be a Manager.");
            }
            employee.AssignManager(dto.ManagerId.Value);
        }

        await _repository.AddAsync(employee);
        await _repository.SaveChangesAsync();

        return MapToResponse(employee);
    }

    public async Task UpdateEmployeeAsync(int id, UpdateEmployeeDTO dto)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) throw new KeyNotFoundException($"Employee with ID {id} not found.");

        employee.UpdateDetails(dto.FirstName, dto.LastName, dto.Phone, dto.Department);
        
        _repository.Update(employee);
        await _repository.SaveChangesAsync();
    }

    public async Task AssignManagerAsync(int employeeId, int managerId)
    {
        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee == null) throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        var manager = await _repository.GetByIdAsync(managerId);
        if (manager == null || manager is not Manager)
        {
            throw new DomainException("Invalid Manager ID. Assigned user must exist and be a Manager.");
        }

        employee.AssignManager(managerId);
        
        _repository.Update(employee);
        await _repository.SaveChangesAsync();
    }

    public async Task RemoveManagerAsync(int employeeId)
    {
        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee == null) throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        employee.RemoveManager();
        
        _repository.Update(employee);
        await _repository.SaveChangesAsync();
    }

    public async Task<EmployeeResponseDTO?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) return null;

        return MapToResponse(employee);
    }

    public async Task<IEnumerable<EmployeeResponseDTO>> GetAllEmployeesAsync()
    {
        var employees = await _repository.GetAllAsync();
        return employees.Select(MapToResponse).ToList();
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) throw new KeyNotFoundException($"Employee with ID {id} not found.");

        _repository.Delete(employee);
        await _repository.SaveChangesAsync();
    }

    private EmployeeResponseDTO MapToResponse(Employee employee)
    {
        string role = employee switch
        {
            Admin => "Admin",
            Manager => "Manager",
            _ => "Employee"
        };

        return new EmployeeResponseDTO(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Phone,
            role,
            employee.Department,
            employee.ManagerId
        );
    }
}
