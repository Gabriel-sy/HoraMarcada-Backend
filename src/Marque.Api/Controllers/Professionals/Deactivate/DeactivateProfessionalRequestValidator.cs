using FluentValidation;

namespace Marque.Api.Controllers.Professionals.Deactivate;

public class DeactivateProfessionalRequestValidator : AbstractValidator<DeactivateProfessionalRequest>
{
    public DeactivateProfessionalRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");
    }
}
