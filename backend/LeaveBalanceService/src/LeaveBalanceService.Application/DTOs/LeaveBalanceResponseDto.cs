using System;

namespace LeaveBalanceService.Application.DTOs;

public class LeaveBalanceResponseDto
{
    public int LeaveBalanceId { get; set; }
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public int TotalDays { get; set; }
    public int CarriedOverDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
    public DateTime ExpirationDate { get; set; }
}
