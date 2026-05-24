using FluentValidation;
using HoraMarcada.Application.Models;

namespace HoraMarcada.Api.Controllers.Appointments.List;

public class ListAppointmentsRequestValidator : AbstractValidator<ListAppointmentsInput>
{
    public ListAppointmentsRequestValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store id is required.");

        RuleFor(x => x.Start)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.End)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.Start).WithMessage("End date must be after or equal to start date.");
    }
}
