namespace Absence.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Absence.Application.Services;
using Absence.Application.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/absences")]
public class AbsencesController : ControllerBase
{
    private readonly AbsenceService _absenceService;

    public AbsencesController(AbsenceService absenceService)
    {
        _absenceService = absenceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AbsenceDto>>> GetAll()
    {
        var absences = await _absenceService.GetAllAsync();
        return Ok(absences);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AbsenceDto>> GetById(int id)
    {
        var absence = await _absenceService.GetByIdAsync(id);
        if (absence == null) return NotFound();
        return Ok(absence);
    }

    [HttpPost("annual")]
    public async Task<ActionResult<int>> CreateAnnual([FromBody] CreateAnnualLeaveDto dto)
    {
        var id = await _absenceService.CreateAnnualLeaveAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPost("sick")]
    public async Task<ActionResult<int>> CreateSick([FromBody] CreateSickLeaveDto dto)
    {
        var id = await _absenceService.CreateSickLeaveAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPost("dayoff")]
    public async Task<ActionResult<int>> CreateDayOff([FromBody] CreateDayOffDto dto)
    {
        var id = await _absenceService.CreateDayOffAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAbsenceDto dto)
    {
        try
        {
            await _absenceService.UpdateAbsenceAsync(id, dto);
            return NoContent();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _absenceService.DeleteAsync(id);
            return NoContent();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/document")]
    public async Task<IActionResult> UploadDocument(int id, Microsoft.AspNetCore.Http.IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        try
        {
            var absence = await _absenceService.GetByIdAsync(id);
            if (absence == null) return NotFound("Absence not found.");

            var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "uploads");
            if (!System.IO.Directory.Exists(uploadsFolder))
            {
                System.IO.Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"absence_{id}_{System.IO.Path.GetFileName(file.FileName)}";
            var filePath = System.IO.Path.Combine(uploadsFolder, fileName);

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (absence.MedicalDocument != true)
            {
                await _absenceService.UpdateAbsenceAsync(id, new UpdateAbsenceDto { Description = absence.Description, MedicalDocument = true });
            }

            return Ok(new { filePath = $"/uploads/{fileName}" });
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/document")]
    public async Task<IActionResult> GetDocument(int id)
    {
        try
        {
            var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "uploads");
            if (!System.IO.Directory.Exists(uploadsFolder)) return NotFound("No documents uploaded yet.");

            var files = System.IO.Directory.GetFiles(uploadsFolder, $"absence_{id}_*");
            if (files.Length == 0) return NotFound("Document not found for this absence.");

            var filePath = files[0];
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(fileBytes, contentType, System.IO.Path.GetFileName(filePath));
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
