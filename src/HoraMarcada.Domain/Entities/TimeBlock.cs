using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Domain.Entities;

public sealed class TimeBlock
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private TimeBlock()
    {
    }

    public static TimeBlock Create(
        Guid storeId,
        Guid professionalId,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        if (storeId == Guid.Empty)
            throw new DomainValidationException("Store id is required.");

        if (professionalId == Guid.Empty)
            throw new DomainValidationException("Professional id is required.");

        if (endTime <= startTime)
            throw new DomainValidationException("End time must be after start time.");

        return new TimeBlock
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ProfessionalId = professionalId,
            StartTime = startTime.ToUniversalTime(),
            EndTime = endTime.ToUniversalTime(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
