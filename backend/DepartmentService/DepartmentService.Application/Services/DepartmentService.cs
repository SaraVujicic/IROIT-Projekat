using DepartmentService.Application.DTOs;
using DepartmentService.Domain.Entities;
using DepartmentService.Domain.Exceptions;
using DepartmentService.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DepartmentService.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<DepartmentDto> GetByIdAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
            throw new KeyNotFoundException($"Department with ID {id} was not found.");

        return MapToDto(department);
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
    {
        var departments = await _departmentRepository.GetAllAsync();
        return departments.Select(MapToDto);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        bool isUnique = await _departmentRepository.IsNameUniqueAsync(dto.Name);
        
        var department = Department.CreateDepartment(dto.Name, dto.Description);
        department.ValidateUniqueName(isUnique);

        await _departmentRepository.AddAsync(department);

        return MapToDto(department);
    }

    public async Task UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
            throw new KeyNotFoundException($"Department with ID {id} was not found.");

        bool isUnique = await _departmentRepository.IsNameUniqueAsync(dto.Name, excludeDepartmentId: id);
        department.ValidateUniqueName(isUnique);

        department.UpdateDepartment(dto.Name, dto.Description);

        await _departmentRepository.UpdateAsync(department);
    }

    public async Task DeleteAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
            throw new KeyNotFoundException($"Department with ID {id} was not found.");

        await _departmentRepository.DeleteAsync(department);
    }

    private static DepartmentDto MapToDto(Department department)
    {
        return new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            Name = department.Name,
            Description = department.Description
        };
    }
}
