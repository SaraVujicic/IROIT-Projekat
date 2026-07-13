using LeaveBalanceService.Application.DTOs;
using LeaveBalanceService.Application.Interfaces;
using LeaveBalanceService.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LeaveBalanceService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveBalancesController : ControllerBase
{
    private readonly ILeaveBalanceService _service;

    public LeaveBalancesController(ILeaveBalanceService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLeaveBalance([FromBody] CreateLeaveBalanceDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetLeaveBalance), new { employeeId = dto.EmployeeId, year = dto.Year }, result);
        }
        catch (ArgumentException ex) { return Conflict(ex.Message); }
        catch (DomainException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{employeeId}/year/{year}")]
    public async Task<IActionResult> GetLeaveBalance(int employeeId, int year)
    {
        var result = await _service.GetByEmployeeAndYearAsync(employeeId, year);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLeaveBalance(int id, [FromBody] UpdateLeaveBalanceDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (NotSupportedException ex) { return BadRequest(ex.Message); }
        catch (DomainException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLeaveBalance(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/use")]
    public async Task<IActionResult> UseLeaveDays(int id, [FromBody] UseLeaveDaysDto dto)
    {
        try
        {
            await _service.UseLeaveDaysAsync(id, dto);
            return NoContent();
        }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (DomainException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("{id}/add")]
    public async Task<IActionResult> AddLeaveDays(int id, [FromBody] AddLeaveDaysDto dto)
    {
        try
        {
            await _service.AddLeaveDaysAsync(id, dto);
            return NoContent();
        }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (DomainException ex) { return BadRequest(ex.Message); }
    }
}
