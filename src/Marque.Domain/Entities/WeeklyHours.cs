using Marque.Domain.Exceptions;

namespace Marque.Domain.Entities;

public sealed class WeeklyHours
{
    public Guid Id { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public int DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public TimeOnly? LunchStart { get; private set; }
    public TimeOnly? LunchEnd { get; private set; }
    public short IntervalMinutes { get; private set; }

    private WeeklyHours()
    {
    }

    public static WeeklyHours Create(
        Guid professionalId,
        int dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        TimeOnly? lunchStart = null,
        TimeOnly? lunchEnd = null,
        short intervalMinutes = 30)
    {
        if (professionalId == Guid.Empty)
            throw new DomainValidationException("Professional id is required.");

        if (dayOfWeek is < 0 or > 6)
            throw new DomainValidationException("Day of week must be between 0 (Sunday) and 6 (Saturday).");

        if (startTime >= endTime)
            throw new DomainValidationException("Start time must be before end time.");

        if (intervalMinutes <= 0)
            throw new DomainValidationException("Interval minutes must be greater than zero.");

        var hasLunchStart = lunchStart.HasValue;
        var hasLunchEnd = lunchEnd.HasValue;
        if (hasLunchStart != hasLunchEnd)
            throw new DomainValidationException("Lunch start and end must both be provided or both omitted.");

        if (hasLunchStart && hasLunchEnd)
        {
            if (lunchStart!.Value >= lunchEnd!.Value)
                throw new DomainValidationException("Lunch start must be before lunch end.");

            if (lunchStart.Value < startTime || lunchEnd.Value > endTime)
                throw new DomainValidationException("Lunch interval must be within work hours.");
        }

        return new WeeklyHours
        {
            Id = Guid.NewGuid(),
            ProfessionalId = professionalId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            LunchStart = lunchStart,
            LunchEnd = lunchEnd,
            IntervalMinutes = intervalMinutes
        };
    }
}
