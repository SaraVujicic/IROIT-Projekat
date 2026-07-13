using System;
using LeaveBalanceService.Domain.Exceptions;

namespace LeaveBalanceService.Domain.Entities;

public sealed class LeaveBalance
{
    public int LeaveBalanceId { get; private set; }
    public int EmployeeId { get; private set; }
    public int Year { get; private set; }
    public int TotalDays { get; private set; }
    public int CarriedOverDays { get; private set; }
    public int RemainingDays { get; private set; }
    public DateTime ExpirationDate { get; private set; }

    // EF Core requires a parameterless constructor
    private LeaveBalance() { }

    public LeaveBalance(int employeeId, int year, int totalDays, int carriedOverDays, int remainingDays, DateTime expirationDate)
    {
        if (totalDays < 0) throw new DomainException("Total days cannot be negative.");
        if (carriedOverDays < 0) throw new DomainException("Carried over days cannot be negative.");
        if (remainingDays < 0) throw new DomainException("Remaining days cannot be negative.");

        EmployeeId = employeeId;
        Year = year;
        TotalDays = totalDays;
        CarriedOverDays = carriedOverDays;
        RemainingDays = remainingDays;
        ExpirationDate = expirationDate.Date;
    }

    public void AddLeaveDays(int days)
    {
        if (days <= 0) throw new DomainException("Days to add must be greater than zero.");
        TotalDays += days;
        RemainingDays += days;
    }

    public void UseLeaveDays(int days)
    {
        if (days <= 0) throw new DomainException("Days to use must be greater than zero.");
        if (days > RemainingDays) throw new DomainException("Cannot use more days than available in remaining balance.");
        RemainingDays -= days;
    }

    public void ExpireCarriedOverDays(DateTime currentDate)
    {
        if (currentDate.Date > ExpirationDate.Date && CarriedOverDays > 0)
        {
            RemainingDays -= CarriedOverDays;
            CarriedOverDays = 0;
            ValidateNoNegativeBalance();
        }
    }

    public LeaveBalance TransferRemainingDaysToNextYear(int nextYear, int nextYearTotalDays, DateTime nextExpirationDate)
    {
        if (nextYear <= Year) throw new DomainException("Next year must be greater than the current year.");

        // remaining days become carried over days for the specified year
        return new LeaveBalance(EmployeeId, nextYear, nextYearTotalDays, RemainingDays, RemainingDays + nextYearTotalDays, nextExpirationDate);
    }

    public void ValidateNoNegativeBalance()
    {
        if (RemainingDays < 0)
        {
            throw new DomainException("Remaining days cannot be negative.");
        }
    }
}
