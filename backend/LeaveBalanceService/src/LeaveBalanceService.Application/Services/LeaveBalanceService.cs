using System;
using System.Threading;
using System.Threading.Tasks;
using LeaveBalanceService.Application.DTOs;
using LeaveBalanceService.Application.Interfaces;
using LeaveBalanceService.Domain.Entities;
using LeaveBalanceService.Domain.Exceptions;
using LeaveBalanceService.Domain.Repositories;

namespace LeaveBalanceService.Application.Services;

public class LeaveBalanceService : ILeaveBalanceService
{
    private readonly ILeaveBalanceRepository _repository;

    public LeaveBalanceService(ILeaveBalanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<LeaveBalanceResponseDto> CreateAsync(CreateLeaveBalanceDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByEmployeeAndYearAsync(dto.EmployeeId, dto.Year, cancellationToken);
        if (existing != null)
        {
            throw new ArgumentException($"Leave balance for employee {dto.EmployeeId} in year {dto.Year} already exists.");
        }

        var entity = new LeaveBalance(dto.EmployeeId, dto.Year, dto.TotalDays, dto.CarriedOverDays, dto.TotalDays + dto.CarriedOverDays, dto.ExpirationDate);
        
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task<LeaveBalanceResponseDto?> GetByEmployeeAndYearAsync(int employeeId, int year, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByEmployeeAndYearAsync(employeeId, year, cancellationToken);
        if (entity == null) return null;

        // Note: For User story 13 - "After expiration date, unused carried-over days are automatically removed"
        // This is a spot to dynamically check and expire if past expiration date.
        // Let's trigger it so reads are always accurate:
        entity.ExpireCarriedOverDays(DateTime.UtcNow);
        
        // If it got updated due to expiration, we should probably save it. But let's keep reads pure if possible, however it's domain logic.
        // Actually, we can just save if there are changes. But EF tracking will handle it next time a save is called or we can explicitly save.
        // We will call save just in case EF core tracked changes
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task<LeaveBalanceResponseDto?> UpdateAsync(int id, UpdateLeaveBalanceDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Generic updates are not supported by the domain model. Use specific actions like Add/Use Leave Days instead.");
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return;

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UseLeaveDaysAsync(int id, UseLeaveDaysDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new ArgumentException($"Leave balance with id {id} not found.");

        entity.UseLeaveDays(dto.DaysToUse);
        
        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task AddLeaveDaysAsync(int id, AddLeaveDaysDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new ArgumentException($"Leave balance with id {id} not found.");

        entity.AddLeaveDays(dto.DaysToAdd);
        
        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static LeaveBalanceResponseDto MapToDto(LeaveBalance entity)
    {
        return new LeaveBalanceResponseDto
        {
            LeaveBalanceId = entity.LeaveBalanceId,
            EmployeeId = entity.EmployeeId,
            Year = entity.Year,
            TotalDays = entity.TotalDays,
            CarriedOverDays = entity.CarriedOverDays,
            UsedDays = entity.TotalDays + entity.CarriedOverDays - entity.RemainingDays,
            RemainingDays = entity.RemainingDays,
            ExpirationDate = entity.ExpirationDate
        };
    }
}
