using FluentValidation;
using Marque.Application.Models;

namespace Marque.Api.Controllers.Appointments.Complete;

public class CompleteAppointmentRequestValidator : AbstractValidator<CompleteAppointmentInput>
{
    public CompleteAppointmentRequestValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store id is required.");
    }
}
