namespace Absence.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Absence.Application.DTOs;
using Absence.Application.Interfaces;
using Absence.Domain.Entities;

public class AbsenceService
{
    private readonly IAbsenceRepository _repository;

    public AbsenceService(IAbsenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CreateAnnualLeaveAsync(CreateAnnualLeaveDto dto)
    {
        var leave = new AnnualLeave(dto.EmployeeId, dto.Description, dto.StartDate, dto.EndDate);
        await _repository.AddAsync(leave);
        return leave.AbsenceId;
    }

    public async Task<int> CreateSickLeaveAsync(CreateSickLeaveDto dto)
    {
        var leave = new SickLeave(dto.EmployeeId, dto.Description, dto.StartDate, dto.EndDate, dto.MedicalDocument);
        await _repository.AddAsync(leave);
        return leave.AbsenceId;
    }

    public async Task<int> CreateDayOffAsync(CreateDayOffDto dto)
    {
        var dayOff = new DayOff(dto.EmployeeId, dto.Description, dto.Date);
        await _repository.AddAsync(dayOff);
        return dayOff.AbsenceId;
    }

    public async Task UpdateAbsenceAsync(int id, UpdateAbsenceDto dto)
    {
        var absence = await _repository.GetByIdAsync(id);
        if (absence == null)
            throw new Exception("Absence not found");

        if (!string.IsNullOrEmpty(dto.Description))
            absence.UpdateDescription(dto.Description);

        switch (absence)
        {
            case AnnualLeave annual:
                // AnnualLeave updates handled in domain service if allowed, omitting manual sets if Private setters
                break;
            case SickLeave sick:
                if (dto.MedicalDocument == true)
                    sick.AttachMedicalDocument();
                break;
            case DayOff dayOff:
                break;
        }

        await _repository.UpdateAsync(absence);
    }

    public async Task<IEnumerable<AbsenceDto>> GetAllAsync()
    {
        var absences = await _repository.GetAllAsync();
        return absences.Select(MapToDto);
    }

    public async Task<AbsenceDto> GetByIdAsync(int id)
    {
        var absence = await _repository.GetByIdAsync(id);
        if (absence == null) return null;
        return MapToDto(absence);
    }

    public async Task DeleteAsync(int id)
    {
        var absence = await _repository.GetByIdAsync(id);
        if (absence == null)
            throw new Exception("Absence not found");
        await _repository.DeleteAsync(absence);
    }

    private AbsenceDto MapToDto(global::Absence.Domain.Entities.Absence absence)
    {
        var dto = new AbsenceDto
        {
            Id = absence.AbsenceId,
            EmployeeId = absence.EmployeeId,
            Description = absence.Description,
            Type = absence.GetType().Name
        };

        if (absence is AnnualLeave annual)
        {
            dto.StartDate = annual.StartDate;
            dto.EndDate = annual.EndDate;
        }
        else if (absence is SickLeave sick)
        {
            dto.StartDate = sick.StartDate;
            dto.EndDate = sick.EndDate;
            dto.MedicalDocument = sick.MedicalDocument;
        }
        else if (absence is DayOff dayOff)
        {
            dto.Date = dayOff.Date;
        }

        return dto;
    }
}
