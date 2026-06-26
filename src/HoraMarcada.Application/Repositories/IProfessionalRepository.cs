using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Repositories;

public interface IProfessionalRepository
{
    Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Professional>> ListByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Professional>> ListAllActiveAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Professional professional, CancellationToken cancellationToken = default);

    Task UpdateAsync(Professional professional, CancellationToken cancellationToken = default);
}
