using Marque.Application.Exceptions;
using Marque.Application.Models;
using Marque.Application.Repositories;
using Marque.Domain.Entities;
using Marque.Domain.ValueObjects;

namespace Marque.Application.UseCases.Stores;

public class CreateStoreUseCase(IStoreRepository storeRepository)
{
    public async Task<Store> ExecuteAsync(Guid userId, CreateStoreInput input, CancellationToken cancellationToken = default)
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
            input.LogoUrl,
            input.Address,
            input.AddressLat,
            input.AddressLng);

        await storeRepository.AddAsync(store, cancellationToken);
        return store;
    }
}
