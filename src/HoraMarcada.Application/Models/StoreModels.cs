using HoraMarcada.Application.Common;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Models;

public record StoreDto(
    Guid Id,
    Guid UserId,
    string Name,
    string Slug,
    string? LogoUrl,
    string? Address,
    decimal? AddressLat,
    decimal? AddressLng,
    string WhatsApp,
    string Email,
    bool Active,
    DateTimeOffset CreatedAt)
{
    public static StoreDto FromDomain(Store store) => new(
        store.Id,
        store.UserId,
        store.Name,
        store.Slug.Value,
        store.LogoUrl,
        store.Address,
        store.AddressLat,
        store.AddressLng,
        store.WhatsApp.Value,
        store.Email.Value,
        store.Active,
        store.CreatedAt);
}

public record CreateStoreInput(
    [property: SwaggerExample("Vintage Barbershop")] string Name,
    [property: SwaggerExample("vintage-barber")] string Slug,
    [property: SwaggerExample("+5511999999999")] string WhatsApp,
    [property: SwaggerExample("contact@vintagebarber.com")] string Email,
    [property: SwaggerExample("https://example.com/logo.png")] string? LogoUrl = null,
    [property: SwaggerExample("123 Main St, São Paulo, SP")] string? Address = null,
    [property: SwaggerExample("-23.5505")] decimal? AddressLat = null,
    [property: SwaggerExample("-46.6333")] decimal? AddressLng = null);

public record UpdateStoreInput(
    [property: SwaggerExample("Vintage Barbershop Premium")] string Name,
    [property: SwaggerExample("vintage-barber-premium")] string Slug,
    [property: SwaggerExample("+5511999999999")] string WhatsApp,
    [property: SwaggerExample("contact-new@vintagebarber.com")] string Email,
    [property: SwaggerExample("https://example.com/logo-new.png")] string? LogoUrl = null,
    [property: SwaggerExample("456 Oak St, São Paulo, SP")] string? Address = null,
    [property: SwaggerExample("-23.5510")] decimal? AddressLat = null,
    [property: SwaggerExample("-46.6340")] decimal? AddressLng = null);
