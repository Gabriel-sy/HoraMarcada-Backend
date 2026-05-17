using Marque.Domain.Exceptions;
using Marque.Domain.ValueObjects;

namespace Marque.Domain.Entities;

public sealed class Service
{
    private readonly HashSet<Guid> _professionalIds = [];

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public short DurationMinutes { get; private set; }
    public Money? Price { get; private set; }
    public bool Active { get; private set; }
    public IReadOnlyCollection<Guid> ProfessionalIds => _professionalIds;

    private Service()
    {
    }

    public static Service Create(
        Guid storeId,
        string name,
        short durationMinutes,
        decimal? price = null,
        bool active = true)
    {
        if (storeId == Guid.Empty)
            throw new DomainValidationException("Store id is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Service name is required.");

        if (durationMinutes <= 0)
            throw new DomainValidationException("Service duration must be greater than zero.");

        return new Service
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            Name = name.Trim(),
            DurationMinutes = durationMinutes,
            Price = Money.CreateOptional(price),
            Active = active
        };
    }

    public void AssignProfessional(Guid professionalId)
    {
        if (professionalId == Guid.Empty)
            throw new DomainValidationException("Professional id is required.");

        _professionalIds.Add(professionalId);
    }
}
