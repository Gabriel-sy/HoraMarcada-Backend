using HoraMarcada.Domain.Exceptions;
using HoraMarcada.Domain.ValueObjects;

namespace HoraMarcada.Domain.Entities;

public sealed class Customer
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public PhoneNumber WhatsApp { get; private set; } = null!;
    public Email? Email { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Customer()
    {
    }

    public static Customer Create(
        Guid storeId,
        string name,
        string whatsApp,
        string? email = null)
    {
        if (storeId == Guid.Empty)
            throw new DomainValidationException("Store id is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Customer name is required.");

        return new Customer
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            Name = name.Trim(),
            WhatsApp = PhoneNumber.Create(whatsApp),
            Email = Email.CreateOptional(email),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string name, string whatsApp, string? email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Customer name is required.");

        Name = name.Trim();
        WhatsApp = PhoneNumber.Create(whatsApp);
        Email = Email.CreateOptional(email);
    }
}
