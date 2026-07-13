using FluentValidation;
using LeaveBalanceService.Application.DTOs;

namespace LeaveBalanceService.Application.Validators;

public class UseLeaveDaysDtoValidator : AbstractValidator<UseLeaveDaysDto>
{
    public UseLeaveDaysDtoValidator()
    {
        RuleFor(x => x.DaysToUse).GreaterThan(0);
    }
}
