using Marque.Domain.Entities;
using Marque.Domain.ValueObjects;

namespace Marque.Application.Repositories;

public interface IStoreRepository
{
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Store?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    Task<Store?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Store store, CancellationToken cancellationToken = default);

    Task UpdateAsync(Store store, CancellationToken cancellationToken = default);
}
