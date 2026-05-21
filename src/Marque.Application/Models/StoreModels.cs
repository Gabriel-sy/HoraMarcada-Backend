namespace Marque.Application.Models;

public record CreateStoreInput(
    Guid UserId,
    string Name,
    string Slug,
    string WhatsApp,
    string? LogoUrl = null,
    string? Address = null,
    decimal? AddressLat = null,
    decimal? AddressLng = null);

public record UpdateStoreInput(
    Guid UserId,
    Guid StoreId,
    string Name,
    string Slug,
    string WhatsApp,
    string? LogoUrl = null,
    string? Address = null,
    decimal? AddressLat = null,
    decimal? AddressLng = null);
