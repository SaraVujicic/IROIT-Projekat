using System.Threading;
using System.Threading.Tasks;
using LeaveBalanceService.Domain.Entities;
using LeaveBalanceService.Domain.Repositories;
using LeaveBalanceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeaveBalanceService.Infrastructure.Repositories;

public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly LeaveBalanceDbContext _context;

    public LeaveBalanceRepository(LeaveBalanceDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveBalance?> GetByIdAsync(int leaveBalanceId, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveBalances
            .FirstOrDefaultAsync(x => x.LeaveBalanceId == leaveBalanceId, cancellationToken);
    }

    public async Task<LeaveBalance?> GetByEmployeeAndYearAsync(int employeeId, int year, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveBalances
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Year == year, cancellationToken);
    }

    public async Task AddAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default)
    {
        await _context.LeaveBalances.AddAsync(leaveBalance, cancellationToken);
    }

    public void Update(LeaveBalance leaveBalance)
    {
        _context.LeaveBalances.Update(leaveBalance);
    }

    public void Delete(LeaveBalance leaveBalance)
    {
        _context.LeaveBalances.Remove(leaveBalance);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
