using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.ValueObjects;

namespace HoraMarcada.Application.UseCases.Stores;

public class UpdateStoreUseCase(
    IStoreRepository storeRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<StoreDto> ExecuteAsync(Guid userId, Guid storeId, UpdateStoreInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var store = await storeRepository.GetByIdAsync(storeId, cancellationToken);
        if (store == null)
        {
            throw new NotFoundException($"Store with ID {storeId} not found.");
        }

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
            input.Email,
            input.LogoUrl,
            input.Address,
            input.AddressLat,
            input.AddressLng,
            store.Active);

        await storeRepository.UpdateAsync(store, cancellationToken);
        return StoreDto.FromDomain(store);
    }
}
