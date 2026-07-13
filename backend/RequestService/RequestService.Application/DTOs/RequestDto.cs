using RequestService.Domain.Enums;

namespace RequestService.Application.DTOs;

public class RequestDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public RequestType Type { get; set; }
    public LeaveType? LeaveType { get; set; }
    public int EmployeeId { get; set; }
    
    // Simplifications for view
    public TravelOrderDto? TravelOrder { get; set; }
    public InternshipDto? Internship { get; set; }
    public EducationDto? Education { get; set; }
}

public class TravelOrderDto
{
    public string Destination { get; set; } = string.Empty;
    public double Costs { get; set; }
}

public class InternshipDto
{
    public string Name { get; set; } = string.Empty;
    public MentorDto? Mentor { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class EducationDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Certificate { get; set; }
}

public class MentorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}
