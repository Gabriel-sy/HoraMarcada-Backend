using FluentValidation;
using HoraMarcada.Application.Models;

namespace HoraMarcada.Api.Controllers.Professionals.Add;

public class AddProfessionalRequestValidator : AbstractValidator<AddProfessionalInput>
{
    public AddProfessionalRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Professional name is required.");

        RuleFor(x => x.WhatsApp)
            .Matches(@"^\+?[1-9]\d{7,14}$")
            .When(x => !string.IsNullOrEmpty(x.WhatsApp))
            .WithMessage("Phone number must be E.164-friendly (optional +, digits only, 8-15 digits).");

        RuleForEach(x => x.WeeklyHours)
            .SetValidator(new WeeklyHoursInputValidator())
            .When(x => x.WeeklyHours != null);
    }
}

public class WeeklyHoursInputValidator : AbstractValidator<WeeklyHoursInput>
{
    public WeeklyHoursInputValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(0, 6).WithMessage("Day of week must be between 0 (Sunday) and 6 (Saturday).");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime).WithMessage("Start time must be before end time.");

        RuleFor(x => x.IntervalMinutes)
            .GreaterThan((short)0).WithMessage("Interval minutes must be greater than zero.");

        RuleFor(x => x)
            .Must(x => (x.LunchStart == null && x.LunchEnd == null) || (x.LunchStart != null && x.LunchEnd != null))
            .WithMessage("Lunch start and end must both be provided or both omitted.")
            .Must(x => x.LunchStart == null || x.LunchEnd == null || x.LunchStart < x.LunchEnd)
            .WithMessage("Lunch start must be before lunch end.")
            .Must(x => x.LunchStart == null || x.LunchEnd == null || (x.LunchStart >= x.StartTime && x.LunchEnd <= x.EndTime))
            .WithMessage("Lunch interval must be within work hours.");
    }
}
