using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Category?> GetByNameAsync(Guid storeId, string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> ListByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);

    Task AddAsync(Category category, CancellationToken cancellationToken = default);
}
