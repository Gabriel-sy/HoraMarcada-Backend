using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.ValueObjects;

namespace HoraMarcada.Application.UseCases.Stores;

public class GetStoreBySlugUseCase(IStoreRepository storeRepository)
{
    public async Task<StoreDto> ExecuteAsync(string slug, CancellationToken cancellationToken = default)
    {
        var store = await storeRepository.GetBySlugAsync(Slug.Create(slug), cancellationToken);
        if (store == null)
        {
            throw new NotFoundException($"Store with slug '{slug}' not found.");
        }
        return StoreDto.FromDomain(store);
    }
}
