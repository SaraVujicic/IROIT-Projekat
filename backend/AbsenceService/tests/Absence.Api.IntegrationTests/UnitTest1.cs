namespace Absence.Api.IntegrationTests;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Absence.Api.Controllers;
using Absence.Application.Services;
using Absence.Application.DTOs;
using Absence.Application.Interfaces;
using Absence.Domain.Entities;
using Xunit;

public class FakeAbsenceRepository : IAbsenceRepository
{
    public System.Collections.Generic.List<global::Absence.Domain.Entities.Absence> Absences { get; } = new();

    public Task<global::Absence.Domain.Entities.Absence> GetByIdAsync(int id)
    {
        return Task.FromResult(Absences.FirstOrDefault(a => a.AbsenceId == id)!);
    }

    public Task<System.Collections.Generic.IEnumerable<global::Absence.Domain.Entities.Absence>> GetAllAsync()
    {
        return Task.FromResult<System.Collections.Generic.IEnumerable<global::Absence.Domain.Entities.Absence>>(Absences);
    }

    public Task AddAsync(global::Absence.Domain.Entities.Absence absence)
    {
        if (absence.AbsenceId == 0)
        {
            var nextId = Absences.Count > 0 ? Absences.Max(a => a.AbsenceId) + 1 : 1;
            typeof(global::Absence.Domain.Entities.Absence)
                .GetProperty("AbsenceId")?
                .SetValue(absence, nextId);
        }
        Absences.Add(absence);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(global::Absence.Domain.Entities.Absence absence)
    {
        var existing = Absences.FirstOrDefault(a => a.AbsenceId == absence.AbsenceId);
        if (existing != null)
        {
            Absences.Remove(existing);
            Absences.Add(absence);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(global::Absence.Domain.Entities.Absence absence)
    {
        Absences.Remove(absence);
        return Task.CompletedTask;
    }
}

public class AbsencesControllerTests : IDisposable
{
    private readonly FakeAbsenceRepository _repository;
    private readonly AbsenceService _service;
    private readonly AbsencesController _controller;
    private readonly string _uploadsFolder;

    public AbsencesControllerTests()
    {
        _repository = new FakeAbsenceRepository();
        _service = new AbsenceService(_repository);
        _controller = new AbsencesController(_service);
        _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    }

    public void Dispose()
    {
        if (Directory.Exists(_uploadsFolder))
        {
            Directory.Delete(_uploadsFolder, true);
        }
    }

    [Fact]
    public async Task UploadDocument_ValidFile_SavesFileAndUpdatesAbsence()
    {
        // 1. Create a SickLeave absence first (initially without document)
        var sickLeave = new SickLeave(1, "Sick with flu", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(2), false);
        await _repository.AddAsync(sickLeave);
        int absenceId = sickLeave.AbsenceId;

        // 2. Prepare fake file
        var content = "This is a test medical document content.";
        var bytes = Encoding.UTF8.GetBytes(content);
        var fileStream = new MemoryStream(bytes);
        var file = new FormFile(fileStream, 0, bytes.Length, "file", "test_report.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        // 3. Act: Upload document
        var result = await _controller.UploadDocument(absenceId, file);

        // 4. Assert response
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Assert file created
        var expectedFileNamePrefix = $"absence_{absenceId}_";
        var files = Directory.GetFiles(_uploadsFolder, $"{expectedFileNamePrefix}*");
        Assert.Single(files);
        Assert.EndsWith("test_report.pdf", files[0]);

        // Assert DB status updated
        var updatedAbsence = (SickLeave)await _repository.GetByIdAsync(absenceId);
        Assert.True(updatedAbsence.MedicalDocument);
    }

    [Fact]
    public async Task GetDocument_ValidAbsence_ReturnsFile()
    {
        // 1. Create a SickLeave
        var sickLeave = new SickLeave(1, "Sick with flu", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(2), true);
        await _repository.AddAsync(sickLeave);
        int absenceId = sickLeave.AbsenceId;

        // Create uploads folder and mock file
        if (!Directory.Exists(_uploadsFolder))
        {
            Directory.CreateDirectory(_uploadsFolder);
        }
        var filePath = Path.Combine(_uploadsFolder, $"absence_{absenceId}_report.pdf");
        var content = "Sample medical note PDF content.";
        await File.WriteAllBytesAsync(filePath, Encoding.UTF8.GetBytes(content));

        // 2. Act: Get document
        var result = await _controller.GetDocument(absenceId);

        // 3. Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"absence_{absenceId}_report.pdf", fileResult.FileDownloadName);
        var fileContent = Encoding.UTF8.GetString(fileResult.FileContents);
        Assert.Equal(content, fileContent);
    }

    [Fact]
    public async Task GetDocument_NoFile_ReturnsNotFound()
    {
        // Act: Get document for non-existent absence
        var result = await _controller.GetDocument(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
