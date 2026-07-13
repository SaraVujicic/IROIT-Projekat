namespace DepartmentService.Domain.Repositories;

using DepartmentService.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id);
    Task<IEnumerable<Department>> GetAllAsync();
    Task<bool> IsNameUniqueAsync(string name, int? excludeDepartmentId = null);
    Task AddAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(Department department);
}
