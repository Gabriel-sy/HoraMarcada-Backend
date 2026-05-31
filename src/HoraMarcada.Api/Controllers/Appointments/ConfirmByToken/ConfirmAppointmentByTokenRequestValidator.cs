using FluentValidation;
using HoraMarcada.Application.Models;

namespace HoraMarcada.Api.Controllers.Appointments.ConfirmByToken;

public class ConfirmAppointmentByTokenRequestValidator : AbstractValidator<ConfirmAppointmentByTokenInput>
{
    public ConfirmAppointmentByTokenRequestValidator()
    {
        RuleFor(x => x.CancelToken)
            .NotEmpty().WithMessage("Cancel token is required.");
    }
}
