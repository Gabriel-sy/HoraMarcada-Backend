using Marque.Domain.Exceptions;
using Marque.Domain.ValueObjects;

namespace Marque.Domain.Entities;

public sealed class Professional
{
    private readonly List<WeeklyHours> _weeklyHours = [];

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? PhotoUrl { get; private set; }
    public PhoneNumber? WhatsApp { get; private set; }
    public bool Active { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<WeeklyHours> WeeklySchedules => _weeklyHours.AsReadOnly();

    private Professional()
    {
    }

    public static Professional Create(
        Guid storeId,
        string name,
        string? photoUrl = null,
        string? whatsApp = null,
        bool active = true)
    {
        if (storeId == Guid.Empty)
            throw new DomainValidationException("Store id is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Professional name is required.");

        return new Professional
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            Name = name.Trim(),
            PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim(),
            WhatsApp = PhoneNumber.CreateOptional(whatsApp),
            Active = active,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public WeeklyHours AddWeeklyHours(
        int dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        TimeOnly? lunchStart = null,
        TimeOnly? lunchEnd = null,
        short intervalMinutes = 30)
    {
        var hours = WeeklyHours.Create(Id, dayOfWeek, startTime, endTime, lunchStart, lunchEnd, intervalMinutes);
        _weeklyHours.Add(hours);
        return hours;
    }

    public void Deactivate()
    {
        Active = false;
    }

    public void Update(string name, string? photoUrl, string? whatsApp)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Professional name is required.");

        Name = name.Trim();
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        WhatsApp = PhoneNumber.CreateOptional(whatsApp);
    }

    public void ClearWeeklyHours()
    {
        _weeklyHours.Clear();
    }
}
