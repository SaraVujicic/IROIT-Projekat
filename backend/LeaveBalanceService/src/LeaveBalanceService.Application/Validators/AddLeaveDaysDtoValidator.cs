using FluentValidation;
using LeaveBalanceService.Application.DTOs;

namespace LeaveBalanceService.Application.Validators;

public class AddLeaveDaysDtoValidator : AbstractValidator<AddLeaveDaysDto>
{
    public AddLeaveDaysDtoValidator()
    {
        RuleFor(x => x.DaysToAdd).GreaterThan(0);
    }
}
