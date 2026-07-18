using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.ValueObjects;

namespace HoraMarcada.Application.UseCases.Stores;

public class CreateStoreUseCase(IStoreRepository storeRepository)
{
    public async Task<StoreDto> ExecuteAsync(Guid userId, CreateStoreInput input, CancellationToken cancellationToken = default)
    {
        var existingStore = await storeRepository.GetBySlugAsync(Slug.Create(input.Slug), cancellationToken);
        if (existingStore != null)
        {
            throw new ConflictException($"Store with slug '{input.Slug}' already exists.");
        }

        var store = Store.Create(
            userId,
            input.Name,
            input.Slug,
            input.WhatsApp,
            input.Email,
            input.LogoUrl,
            input.Address,
            input.AddressLat,
            input.AddressLng);

        await storeRepository.AddAsync(store, cancellationToken);
        return StoreDto.FromDomain(store);
    }
}
