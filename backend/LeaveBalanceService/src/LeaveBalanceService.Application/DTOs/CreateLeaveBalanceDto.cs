using System;

namespace LeaveBalanceService.Application.DTOs;

public class CreateLeaveBalanceDto
{
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public int TotalDays { get; set; }
    public int CarriedOverDays { get; set; }
    public DateTime ExpirationDate { get; set; }
}
