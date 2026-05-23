using Marque.Application.Exceptions;
using Marque.Application.Models;
using Marque.Application.Policies;
using Marque.Application.Repositories;
using Marque.Domain.Entities;
using Marque.Domain.ValueObjects;

namespace Marque.Application.UseCases.Stores;

public class UpdateStoreUseCase(
    IStoreRepository storeRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<Store> ExecuteAsync(Guid storeId, UpdateStoreInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(input.UserId, storeId, cancellationToken);

        var store = await storeRepository.GetByIdAsync(storeId, cancellationToken);
        if (store == null)
        {
            throw new NotFoundException($"Store with ID {storeId} not found.");
        }

        // Check for slug uniqueness if changed
        var inputSlug = Slug.Create(input.Slug);
        if (store.Slug != inputSlug)
        {
            var existingStore = await storeRepository.GetBySlugAsync(inputSlug, cancellationToken);
            if (existingStore != null && existingStore.Id != store.Id)
            {
                throw new ConflictException($"Store with slug '{input.Slug}' already exists.");
            }
        }

        store.Update(
            input.Name,
            input.Slug,
            input.WhatsApp,
            input.LogoUrl,
            input.Address,
            input.AddressLat,
            input.AddressLng,
            store.Active);

        await storeRepository.UpdateAsync(store, cancellationToken);
        return store;
    }
}
