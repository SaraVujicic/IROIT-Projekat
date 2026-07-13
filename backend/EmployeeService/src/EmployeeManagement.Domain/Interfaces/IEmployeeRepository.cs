using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Domain.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByUsernameAsync(string username);
    Task<IEnumerable<Employee>> GetAllAsync();
    Task AddAsync(Employee employee);
    void Update(Employee employee);
    void Delete(Employee employee);
    Task SaveChangesAsync();
}
