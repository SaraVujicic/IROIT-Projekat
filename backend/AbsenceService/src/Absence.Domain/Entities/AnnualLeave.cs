namespace Absence.Domain.Entities;

using System;

public class AnnualLeave : Absence
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    private AnnualLeave() { } // EF Core

    public AnnualLeave(int employeeId, string description, DateTime startDate, DateTime endDate) 
        : base(employeeId, description)
    {
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date.");
            
        StartDate = startDate;
        EndDate = endDate;
    }
}
