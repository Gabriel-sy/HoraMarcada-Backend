using Marque.Domain.Entities;

namespace Marque.Application.Repositories;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> ListByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);

    Task AddAsync(Service service, CancellationToken cancellationToken = default);
}
