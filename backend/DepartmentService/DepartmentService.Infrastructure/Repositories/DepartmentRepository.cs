namespace DepartmentService.Infrastructure.Repositories;

using DepartmentService.Domain.Entities;
using DepartmentService.Domain.Repositories;
using DepartmentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly DepartmentDbContext _dbContext;

    public DepartmentRepository(DepartmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _dbContext.Departments.FindAsync(id);
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _dbContext.Departments.ToListAsync();
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeDepartmentId = null)
    {
        var query = _dbContext.Departments.AsQueryable();
        
        if (excludeDepartmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId != excludeDepartmentId.Value);
        }

        return !await query.AnyAsync(d => d.Name == name);
    }

    public async Task AddAsync(Department department)
    {
        await _dbContext.Departments.AddAsync(department);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        _dbContext.Departments.Update(department);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Department department)
    {
        _dbContext.Departments.Remove(department);
        await _dbContext.SaveChangesAsync();
    }
}
