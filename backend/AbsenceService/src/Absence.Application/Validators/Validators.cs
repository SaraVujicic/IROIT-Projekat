namespace Absence.Application.Validators;

using FluentValidation;
using Absence.Application.DTOs;

public class CreateAnnualLeaveValidator : AbstractValidator<CreateAnnualLeaveDto>
{
    public CreateAnnualLeaveValidator()
    {
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
    }
}

public class CreateSickLeaveValidator : AbstractValidator<CreateSickLeaveDto>
{
    public CreateSickLeaveValidator()
    {
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.MedicalDocument).Equal(true).WithMessage("SickLeave requires a medical document.");
    }
}

public class CreateDayOffValidator : AbstractValidator<CreateDayOffDto>
{
    public CreateDayOffValidator()
    {
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
    }
}
