using System;
using FluentAssertions;
using LeaveBalanceService.Domain.Entities;
using LeaveBalanceService.Domain.Exceptions;
using Xunit;

namespace LeaveBalanceService.Tests.Unit;

public class LeaveBalanceAggregateTests
{
    [Fact]
    public void Create_ValidParameters_ShouldCreateLeaveBalance()
    {
        var balance = new LeaveBalance(1, 2024, 20, 5, 25, new DateTime(2025, 6, 30));

        balance.RemainingDays.Should().Be(25);
    }

    [Fact]
    public void Create_NegativeTotalDays_ShouldThrowDomainException()
    {
        Action act = () => new LeaveBalance(1, 2024, -1, 5, 4, new DateTime(2025, 6, 30));

        act.Should().Throw<DomainException>().WithMessage("Total days cannot be negative.");
    }

    [Fact]
    public void AddLeaveDays_ShouldIncreaseTotalDays()
    {
        var balance = new LeaveBalance(1, 2024, 20, 5, 25, new DateTime(2025, 6, 30));
        
        balance.AddLeaveDays(2);

        balance.TotalDays.Should().Be(22);
        balance.RemainingDays.Should().Be(27);
    }

    [Fact]
    public void UseLeaveDays_WhenMoreDaysUsedThanAvailable_ShouldThrowException()
    {
        var balance = new LeaveBalance(1, 2024, 20, 0, 20, new DateTime(2025, 6, 30));
        
        Action act = () => balance.UseLeaveDays(21);

        act.Should().Throw<DomainException>().WithMessage("Cannot use more days than available in remaining balance.");
    }

    [Fact]
    public void UseLeaveDays_ValidAmount_ShouldDecreaseRemainingDays()
    {
        var balance = new LeaveBalance(1, 2024, 20, 5, 25, new DateTime(2025, 6, 30));
        
        balance.UseLeaveDays(10);

        balance.RemainingDays.Should().Be(15);
    }

    [Fact]
    public void ExpireCarriedOverDays_AfterExpirationDate_ShouldRemoveCarriedOverDays()
    {
        var expirationDate = new DateTime(2024, 6, 30);
        var currentDate = new DateTime(2024, 7, 1);
        var balance = new LeaveBalance(1, 2024, 20, 5, 25, expirationDate);

        balance.ExpireCarriedOverDays(currentDate);

        balance.CarriedOverDays.Should().Be(0);
        balance.RemainingDays.Should().Be(20);
    }

    [Fact]
    public void ExpireCarriedOverDays_BeforeExpirationDate_ShouldNotRemoveCarriedOverDays()
    {
        var expirationDate = new DateTime(2024, 6, 30);
        var currentDate = new DateTime(2024, 6, 29);
        var balance = new LeaveBalance(1, 2024, 20, 5, 25, expirationDate);

        balance.ExpireCarriedOverDays(currentDate);

        balance.CarriedOverDays.Should().Be(5);
        balance.RemainingDays.Should().Be(25);
    }
}
