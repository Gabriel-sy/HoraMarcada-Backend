using Marque.Domain.Entities;

namespace Marque.Application.Repositories;

public interface IProfessionalRepository
{
    Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Professional>> ListByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);

    Task AddAsync(Professional professional, CancellationToken cancellationToken = default);
}
