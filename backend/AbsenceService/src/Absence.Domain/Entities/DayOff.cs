namespace Absence.Domain.Entities;

using System;

public class DayOff : Absence
{
    public DateTime Date { get; private set; }

    private DayOff() { } // EF Core

    public DayOff(int employeeId, string description, DateTime date) 
        : base(employeeId, description)
    {
        Date = date;
    }
}
