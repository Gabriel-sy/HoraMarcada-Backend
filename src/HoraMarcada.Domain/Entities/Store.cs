using HoraMarcada.Domain.Exceptions;
using HoraMarcada.Domain.ValueObjects;

namespace HoraMarcada.Domain.Entities;

public sealed class Store
{
    private readonly List<Professional> _professionals = [];
    private readonly List<Service> _services = [];

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public string? LogoUrl { get; private set; }
    public string? Address { get; private set; }
    public decimal? AddressLat { get; private set; }
    public decimal? AddressLng { get; private set; }
    public PhoneNumber WhatsApp { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public bool Active { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<Professional> Professionals => _professionals.AsReadOnly();
    public IReadOnlyCollection<Service> Services => _services.AsReadOnly();

    private Store()
    {
    }

    public static Store Create(
        Guid userId,
        string name,
        string slug,
        string whatsApp,
        string email,
        string? logoUrl = null,
        string? address = null,
        decimal? addressLat = null,
        decimal? addressLng = null,
        bool active = true)
    {
        if (userId == Guid.Empty)
            throw new DomainValidationException("User id is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Store name is required.");

        return new Store
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Slug = Slug.Create(slug),
            LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim(),
            Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
            AddressLat = addressLat,
            AddressLng = addressLng,
            WhatsApp = PhoneNumber.Create(whatsApp),
            Email = Email.Create(email),
            Active = active,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public Professional AddProfessional(
        string name,
        string? photoUrl = null,
        string? whatsApp = null,
        bool active = true)
    {
        var professional = Professional.Create(Id, name, photoUrl, whatsApp, active);
        _professionals.Add(professional);
        return professional;
    }

    public Service AddService(
        Guid categoryId,
        string name,
        short durationMinutes,
        decimal? price = null,
        bool active = true)
    {
        var service = Service.Create(Id, categoryId, name, durationMinutes, price, active);
        _services.Add(service);
        return service;
    }

    public void Update(
        string name,
        string slug,
        string whatsApp,
        string email,
        string? logoUrl,
        string? address,
        decimal? addressLat,
        decimal? addressLng,
        bool active)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Store name is required.");

        Name = name.Trim();
        Slug = Slug.Create(slug);
        WhatsApp = PhoneNumber.Create(whatsApp);
        Email = Email.Create(email);
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        AddressLat = addressLat;
        AddressLng = addressLng;
        Active = active;
    }
}
