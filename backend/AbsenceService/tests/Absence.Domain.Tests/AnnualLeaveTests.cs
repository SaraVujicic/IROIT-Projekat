namespace Absence.Domain.Tests;

using System;
using Absence.Domain.Entities;
using Xunit;

public class AnnualLeaveTests
{
    [Fact]
    public void Create_ValidDates_ShouldCreateInstance()
    {
        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(2);

        var leave = new AnnualLeave(1, "Vacation", startDate, endDate);

        Assert.NotNull(leave);
        Assert.Equal("Vacation", leave.Description);
        Assert.Equal(startDate, leave.StartDate);
        Assert.Equal(endDate, leave.EndDate);
    }

    [Fact]
    public void Create_StartDateAfterEndDate_ShouldThrowArgumentException()
    {
        var startDate = DateTime.Now.AddDays(2);
        var endDate = DateTime.Now;

        Assert.Throws<ArgumentException>(() => new AnnualLeave(1, "Invalid Vacation", startDate, endDate));
    }
}

public class SickLeaveTests
{
    [Fact]
    public void Create_SickLeave_StoresMedicalDocument()
    {
        var leave = new SickLeave(1, "Flu", DateTime.Now, DateTime.Now.AddDays(1), true);

        Assert.True(leave.MedicalDocument);
    }
}
