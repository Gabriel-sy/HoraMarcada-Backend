using FluentValidation;
using HoraMarcada.Application.Models;

namespace HoraMarcada.Api.Controllers.Services.Add;

public class AddServiceRequestValidator : AbstractValidator<AddServiceInput>
{
    public AddServiceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category id is required.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan((short)0).WithMessage("Service duration must be greater than zero.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.")
            .When(x => x.Price.HasValue);
    }
}
