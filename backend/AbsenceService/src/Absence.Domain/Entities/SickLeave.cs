namespace Absence.Domain.Entities;

using System;

public class SickLeave : Absence
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool MedicalDocument { get; private set; }

    private SickLeave() { } // EF Core

    public SickLeave(int employeeId, string description, DateTime startDate, DateTime endDate, bool medicalDocument) 
        : base(employeeId, description)
    {
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date.");

        StartDate = startDate;
        EndDate = endDate;
        MedicalDocument = medicalDocument;
    }

    public void AttachMedicalDocument()
    {
        MedicalDocument = true;
    }
}
