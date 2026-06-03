using FluentValidation;
using HoraMarcada.Application.Models;

namespace HoraMarcada.Api.Controllers.Customers.Update;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerInput>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.WhatsApp)
            .NotEmpty().WithMessage("WhatsApp is required.")
            .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Phone number must be E.164-friendly.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
