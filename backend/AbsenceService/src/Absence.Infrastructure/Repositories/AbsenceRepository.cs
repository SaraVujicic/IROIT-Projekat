namespace Absence.Infrastructure.Repositories;

using Absence.Application.Interfaces;
using Absence.Domain.Entities;
using Absence.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AbsenceRepository : IAbsenceRepository
{
    private readonly AbsenceDbContext _context;

    public AbsenceRepository(AbsenceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(global::Absence.Domain.Entities.Absence absence)
    {
        await _context.Absences.AddAsync(absence);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(global::Absence.Domain.Entities.Absence absence)
    {
        _context.Absences.Remove(absence);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<global::Absence.Domain.Entities.Absence>> GetAllAsync()
    {
        return await _context.Absences.ToListAsync();
    }

    public async Task<global::Absence.Domain.Entities.Absence> GetByIdAsync(int id)
    {
        return await _context.Absences.FindAsync(id);
    }

    public async Task UpdateAsync(global::Absence.Domain.Entities.Absence absence)
    {
        _context.Absences.Update(absence);
        await _context.SaveChangesAsync();
    }
}
