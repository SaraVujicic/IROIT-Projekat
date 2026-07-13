using FluentValidation;
using LeaveBalanceService.Application.DTOs;

namespace LeaveBalanceService.Application.Validators;

public class CreateLeaveBalanceDtoValidator : AbstractValidator<CreateLeaveBalanceDto>
{
    public CreateLeaveBalanceDtoValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
        RuleFor(x => x.Year).GreaterThan(1900).LessThan(2100);
        RuleFor(x => x.TotalDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CarriedOverDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExpirationDate).NotEmpty();
    }
}
