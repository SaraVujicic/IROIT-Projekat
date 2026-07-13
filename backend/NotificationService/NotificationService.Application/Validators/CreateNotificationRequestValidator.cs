using FluentValidation;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Validators
{
    public class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
    {
        public CreateNotificationRequestValidator()
        {
            RuleFor(x => x.RecipientId)
                .GreaterThan(0).WithMessage("A valid RecipientId must be provided.");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required.");
        }
    }
}
