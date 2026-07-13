namespace Absence.Application.DTOs;

public class AbsenceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Description { get; set; }
    public string Type { get; set; } 
    
    public System.DateTime? StartDate { get; set; }
    public System.DateTime? EndDate { get; set; }
    public bool? MedicalDocument { get; set; }
    public System.DateTime? Date { get; set; }
}

public class CreateAnnualLeaveDto
{
    public int EmployeeId { get; set; }
    public string Description { get; set; }
    public System.DateTime StartDate { get; set; }
    public System.DateTime EndDate { get; set; }
}

public class CreateSickLeaveDto
{
    public int EmployeeId { get; set; }
    public string Description { get; set; }
    public System.DateTime StartDate { get; set; }
    public System.DateTime EndDate { get; set; }
    public bool MedicalDocument { get; set; }
}

public class CreateDayOffDto
{
    public int EmployeeId { get; set; }
    public string Description { get; set; }
    public System.DateTime Date { get; set; }
}

public class UpdateAbsenceDto
{
    public string Description { get; set; }
    
    // Optional properties for specific types
    public System.DateTime? StartDate { get; set; }
    public System.DateTime? EndDate { get; set; }
    public bool? MedicalDocument { get; set; }
    public System.DateTime? Date { get; set; }
}
