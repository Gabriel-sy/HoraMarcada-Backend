using FluentValidation;
using HoraMarcada.Application.Models;

namespace HoraMarcada.Api.Controllers.Appointments.CancelByToken;

public class CancelAppointmentByTokenRequestValidator : AbstractValidator<CancelAppointmentByTokenInput>
{
    public CancelAppointmentByTokenRequestValidator()
    {
        RuleFor(x => x.CancelToken)
            .NotEmpty().WithMessage("Cancel token is required.");
    }
}
