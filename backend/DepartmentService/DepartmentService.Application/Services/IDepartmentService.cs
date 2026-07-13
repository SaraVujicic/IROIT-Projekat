using DepartmentService.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepartmentService.Application.Services;

public interface IDepartmentService
{
    Task<DepartmentDto> GetByIdAsync(int id);
    Task<IEnumerable<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task UpdateAsync(int id, UpdateDepartmentDto dto);
    Task DeleteAsync(int id);
}
