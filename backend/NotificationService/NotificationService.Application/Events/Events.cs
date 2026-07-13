using System;

namespace NotificationService.Application.Events;

public class RequestCreatedEvent
{
    public int RequestId { get; set; }
    public int EmployeeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string RequestType { get; set; } = string.Empty;
}

public class RequestStatusChangedEvent
{
    public int RequestId { get; set; }
    public int EmployeeId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? ApproverRole { get; set; }
    public string Description { get; set; } = string.Empty;
}
