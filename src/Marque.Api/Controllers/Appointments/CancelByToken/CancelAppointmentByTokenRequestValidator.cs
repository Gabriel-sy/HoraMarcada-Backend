using FluentValidation;
using Marque.Application.Models;

namespace Marque.Api.Controllers.Appointments.CancelByToken;

public class CancelAppointmentByTokenRequestValidator : AbstractValidator<CancelAppointmentByTokenInput>
{
    public CancelAppointmentByTokenRequestValidator()
    {
        RuleFor(x => x.CancelToken)
            .NotEmpty().WithMessage("Cancel token is required.");
    }
}
