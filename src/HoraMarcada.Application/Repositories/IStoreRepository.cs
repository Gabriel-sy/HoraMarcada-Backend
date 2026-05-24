using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.ValueObjects;

namespace HoraMarcada.Application.Repositories;

public interface IStoreRepository
{
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Store?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    Task<Store?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Store store, CancellationToken cancellationToken = default);

    Task UpdateAsync(Store store, CancellationToken cancellationToken = default);
}
