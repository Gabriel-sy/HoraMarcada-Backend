using FluentValidation;
using HoraMarcada.Application.Models;

namespace HoraMarcada.Api.Controllers.Appointments.Complete;

public class CompleteAppointmentRequestValidator : AbstractValidator<CompleteAppointmentInput>
{
    public CompleteAppointmentRequestValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store id is required.");
    }
}
