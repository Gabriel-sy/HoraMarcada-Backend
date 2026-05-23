using FluentValidation;
using Marque.Application.Models;

namespace Marque.Api.Controllers.Stores.Create;

public class CreateStoreRequestValidator : AbstractValidator<CreateStoreInput>
{
    public CreateStoreRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store name is required.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.WhatsApp)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Phone number must be E.164-friendly (optional +, digits only, 8-15 digits).");
    }
}
