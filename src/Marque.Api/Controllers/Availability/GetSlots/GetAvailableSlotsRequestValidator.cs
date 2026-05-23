using FluentValidation;
using Marque.Application.Models;

namespace Marque.Api.Controllers.Availability.GetSlots;

public class GetAvailableSlotsRequestValidator : AbstractValidator<GetAvailableSlotsInput>
{
    public GetAvailableSlotsRequestValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service id is required.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");
    }
}
