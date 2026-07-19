using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoraMarcada.Infrastructure.Persistence.Repositories;

internal sealed class EfCategoryRepository(HoraMarcadaDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(Guid storeId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return await context.Categories
            .FirstOrDefaultAsync(
                c => c.StoreId == storeId && c.Name.ToLower() == normalized,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> ListByStoreIdAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        return await context.Categories
            .Where(c => c.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await context.Categories.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
