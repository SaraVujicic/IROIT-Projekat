using FluentValidation;

namespace RequestService.Application.Commands.CreateLeaveRequest;

public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(v => v.EmployeeId)
            .GreaterThan(0).WithMessage("EmployeeId is required.");

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(v => v.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(v => v.EndDate)
            .NotEmpty().WithMessage("EndDate is required.")
            .GreaterThanOrEqualTo(v => v.StartDate).WithMessage("EndDate must be after or equal to StartDate.");
    }
}
