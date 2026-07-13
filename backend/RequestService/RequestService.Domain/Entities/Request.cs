using RequestService.Domain.Common;
using RequestService.Domain.Enums;
using RequestService.Domain.ValueObjects;

namespace RequestService.Domain.Entities;

public class Request : Entity
{
    // Emulates the Request Aggregate Root
    public int RequestId { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public StatusVO Status { get; private set; }
    public RequestType Type { get; private set; }
    
    // Relation to employee
    public int EmployeeId { get; private set; }

    // Audit snapshots
    public ManagerCredentialsVO? ManagerAudit { get; private set; }
    public AdminCredentialsVO? AdminAudit { get; private set; }

    // Navigation properties for child entities
    private readonly List<TravelOrder> _travelOrders = new();
    public IReadOnlyCollection<TravelOrder> TravelOrders => _travelOrders.AsReadOnly();

    private readonly List<Internship> _internships = new();
    public IReadOnlyCollection<Internship> Internships => _internships.AsReadOnly();

    private readonly List<Education> _educations = new();
    public IReadOnlyCollection<Education> Educations => _educations.AsReadOnly();
    
    // For Leave requests
    public LeaveType? LeaveType { get; private set; }

    private Request() { } // EF Core

    // Factory method for Leave
    public static Request SubmitLeaveRequest(int employeeId, string description, DateTime startDate, DateTime endDate, LeaveType leaveType)
    {
        ValidateDates(startDate, endDate);
        return new Request
        {
            EmployeeId = employeeId,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Type = RequestType.Leave,
            LeaveType = leaveType,
            Status = StatusVO.Pending
        };
    }

    // Factory method for Travel Order
    public static Request SubmitTravelOrder(int employeeId, string description, DateTime startDate, DateTime endDate, TravelOrder travelOrder)
    {
        ValidateDates(startDate, endDate);
        var request = new Request
        {
            EmployeeId = employeeId,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Type = RequestType.TravelOrder,
            Status = StatusVO.Pending
        };
        request._travelOrders.Add(travelOrder ?? throw new ArgumentNullException(nameof(travelOrder)));
        return request;
    }

    // Factory method for Internship
    public static Request SubmitInternship(int employeeId, string description, DateTime startDate, DateTime endDate, Internship internship)
    {
        ValidateDates(startDate, endDate);
        var request = new Request
        {
            EmployeeId = employeeId,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Type = RequestType.Internship,
            Status = StatusVO.Pending
        };
        request._internships.Add(internship ?? throw new ArgumentNullException(nameof(internship)));
        return request;
    }

    // Factory method for Education
    public static Request SubmitEducation(int employeeId, string description, DateTime startDate, DateTime endDate, Education education)
    {
        ValidateDates(startDate, endDate);
        var request = new Request
        {
            EmployeeId = employeeId,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Type = RequestType.Education,
            Status = StatusVO.Pending
        };
        request._educations.Add(education ?? throw new ArgumentNullException(nameof(education)));
        return request;
    }

    private static void ValidateDates(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("StartDate cannot be after EndDate");
    }

    public void ApproveByManager(ManagerCredentialsVO managerAudit)
    {
        if (Status != StatusVO.Pending)
            throw new InvalidOperationException("Only pending requests can be approved by manager.");
        Status = StatusVO.PendingAdminApproval;
        ManagerAudit = managerAudit ?? throw new ArgumentNullException(nameof(managerAudit));
    }

    public void ApproveByAdmin(AdminCredentialsVO adminAudit)
    {
        if (Status != StatusVO.PendingAdminApproval && Status != StatusVO.Pending)
            throw new InvalidOperationException("Request cannot be approved by admin from current status.");
        Status = StatusVO.Approved;
        AdminAudit = adminAudit ?? throw new ArgumentNullException(nameof(adminAudit));
    }

    public void Reject()
    {
        Status = StatusVO.Rejected;
    }
}
