using FluentValidation;
using Marque.Application.Models;
using Marque.Api.Controllers.Professionals.Add; // We can import WeeklyHoursInputValidator or redefine it. Let's redefine for folder-level self-containment.

namespace Marque.Api.Controllers.Professionals.Update;

public class UpdateProfessionalRequestValidator : AbstractValidator<UpdateProfessionalInput>
{
    public UpdateProfessionalRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Professional name is required.");

        RuleFor(x => x.WhatsApp)
            .Matches(@"^\+?[1-9]\d{7,14}$")
            .When(x => !string.IsNullOrEmpty(x.WhatsApp))
            .WithMessage("Phone number must be E.164-friendly (optional +, digits only, 8-15 digits).");

        RuleForEach(x => x.WeeklyHours)
            .SetValidator(new WeeklyHoursInputValidator())
            .When(x => x.WeeklyHours != null);
    }
}
