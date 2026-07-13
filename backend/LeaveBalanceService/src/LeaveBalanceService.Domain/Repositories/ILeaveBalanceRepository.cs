using System.Threading;
using System.Threading.Tasks;
using LeaveBalanceService.Domain.Entities;

namespace LeaveBalanceService.Domain.Repositories;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetByIdAsync(int leaveBalanceId, CancellationToken cancellationToken = default);
    Task<LeaveBalance?> GetByEmployeeAndYearAsync(int employeeId, int year, CancellationToken cancellationToken = default);
    Task AddAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default);
    void Update(LeaveBalance leaveBalance);
    void Delete(LeaveBalance leaveBalance);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
