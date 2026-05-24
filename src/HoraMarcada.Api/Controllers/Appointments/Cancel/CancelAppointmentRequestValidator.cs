using FluentValidation;
using HoraMarcada.Application.Models;

namespace HoraMarcada.Api.Controllers.Appointments.Cancel;

public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentInput>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store id is required.");
    }
}
