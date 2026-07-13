using FluentValidation;
using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Validators;

public class CreateEmployeeDTOValidator : AbstractValidator<CreateEmployeeDTO>
{
    public CreateEmployeeDTOValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Role).Must(x => x == "Employee" || x == "Manager" || x == "Admin")
            .WithMessage("Role must be Employee, Manager, or Admin.");
    }
}

public class UpdateEmployeeDTOValidator : AbstractValidator<UpdateEmployeeDTO>
{
    public UpdateEmployeeDTOValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
    }
}

public class LoginDTOValidator : AbstractValidator<LoginDTO>
{
    public LoginDTOValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
