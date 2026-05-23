using FluentValidation;
using Marque.Application.Models;

namespace Marque.Api.Controllers.Appointments.Book;

public class BookAppointmentRequestValidator : AbstractValidator<BookAppointmentInput>
{
    public BookAppointmentRequestValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store id is required.");

        RuleFor(x => x.ProfessionalId)
            .NotEmpty().WithMessage("Professional id is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service id is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.");

        RuleFor(x => x.CustomerWhatsApp)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Phone number must be E.164-friendly (optional +, digits only, 8-15 digits).");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.");
    }
}
