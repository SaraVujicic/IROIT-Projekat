namespace Absence.Domain.Entities;

public abstract class Absence
{
    public int AbsenceId { get; protected set; } // Root Identity
    public int EmployeeId { get; protected set; } // Association
    public string Description { get; protected set; }

    protected Absence() { } // For EF Core

    protected Absence(int employeeId, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new System.ArgumentException("Description cannot be empty.", nameof(description));
        EmployeeId = employeeId;
        Description = description;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new System.ArgumentException("Description cannot be empty.", nameof(description));
        Description = description;
    }
}
