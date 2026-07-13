using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeResponseDTO> CreateEmployeeAsync(CreateEmployeeDTO dto);
    Task UpdateEmployeeAsync(int id, UpdateEmployeeDTO dto);
    Task AssignManagerAsync(int employeeId, int managerId);
    Task RemoveManagerAsync(int employeeId);
    Task<EmployeeResponseDTO?> GetEmployeeByIdAsync(int id);
    Task<IEnumerable<EmployeeResponseDTO>> GetAllEmployeesAsync();
    Task DeleteEmployeeAsync(int id);
}
