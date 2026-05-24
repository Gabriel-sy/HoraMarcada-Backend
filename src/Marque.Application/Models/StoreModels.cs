using Marque.Application.Common;

namespace Marque.Application.Models;

public record CreateStoreInput(
    [property: SwaggerExample("Vintage Barbershop")] string Name,
    [property: SwaggerExample("vintage-barber")] string Slug,
    [property: SwaggerExample("+5511999999999")] string WhatsApp,
    [property: SwaggerExample("https://example.com/logo.png")] string? LogoUrl = null,
    [property: SwaggerExample("123 Main St, São Paulo, SP")] string? Address = null,
    [property: SwaggerExample("-23.5505")] decimal? AddressLat = null,
    [property: SwaggerExample("-46.6333")] decimal? AddressLng = null);

public record UpdateStoreInput(
    [property: SwaggerExample("Vintage Barbershop Premium")] string Name,
    [property: SwaggerExample("vintage-barber-premium")] string Slug,
    [property: SwaggerExample("+5511999999999")] string WhatsApp,
    [property: SwaggerExample("https://example.com/logo-new.png")] string? LogoUrl = null,
    [property: SwaggerExample("456 Oak St, São Paulo, SP")] string? Address = null,
    [property: SwaggerExample("-23.5510")] decimal? AddressLat = null,
    [property: SwaggerExample("-46.6340")] decimal? AddressLng = null);
