using Marque.Application.Repositories;
using Marque.Domain.Entities;
using Marque.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Marque.Infrastructure.Persistence.Repositories;

internal sealed class EfStoreRepository(MarqueDbContext context) : IStoreRepository
{
    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Stores
            .Include(s => s.Professionals)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Store?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
    {
        return await context.Stores
            .Include(s => s.Professionals)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);
    }

    public async Task AddAsync(Store store, CancellationToken cancellationToken = default)
    {
        await context.Stores.AddAsync(store, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
