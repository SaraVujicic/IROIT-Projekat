namespace Absence.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAbsenceRepository
{
    Task<global::Absence.Domain.Entities.Absence> GetByIdAsync(int id);
    Task<IEnumerable<global::Absence.Domain.Entities.Absence>> GetAllAsync();
    Task AddAsync(global::Absence.Domain.Entities.Absence absence);
    Task UpdateAsync(global::Absence.Domain.Entities.Absence absence);
    Task DeleteAsync(global::Absence.Domain.Entities.Absence absence);
}
