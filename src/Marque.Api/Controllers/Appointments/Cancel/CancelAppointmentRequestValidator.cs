using FluentValidation;
using Marque.Application.Models;

namespace Marque.Api.Controllers.Appointments.Cancel;

public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentInput>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store id is required.");
    }
}
