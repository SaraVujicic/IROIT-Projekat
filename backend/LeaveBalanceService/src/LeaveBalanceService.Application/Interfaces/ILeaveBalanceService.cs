using System.Threading;
using System.Threading.Tasks;
using LeaveBalanceService.Application.DTOs;

namespace LeaveBalanceService.Application.Interfaces;

public interface ILeaveBalanceService
{
    Task<LeaveBalanceResponseDto> CreateAsync(CreateLeaveBalanceDto dto, CancellationToken cancellationToken = default);
    Task<LeaveBalanceResponseDto?> GetByEmployeeAndYearAsync(int employeeId, int year, CancellationToken cancellationToken = default);
    Task<LeaveBalanceResponseDto?> UpdateAsync(int id, UpdateLeaveBalanceDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task UseLeaveDaysAsync(int id, UseLeaveDaysDto dto, CancellationToken cancellationToken = default);
    Task AddLeaveDaysAsync(int id, AddLeaveDaysDto dto, CancellationToken cancellationToken = default);
}
