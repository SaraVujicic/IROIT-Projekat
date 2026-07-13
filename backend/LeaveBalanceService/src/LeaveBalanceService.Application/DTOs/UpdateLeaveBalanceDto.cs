using System;

namespace LeaveBalanceService.Application.DTOs;

public class UpdateLeaveBalanceDto
{
    public int? TotalDays { get; set; }
    public int? CarriedOverDays { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
