using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HoraMarcada.Infrastructure.Persistence.Repositories;

internal sealed class EfStoreRepository(HoraMarcadaDbContext context) : IStoreRepository
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

    public async Task<Store?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Stores
            .Include(s => s.Professionals)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Store store, CancellationToken cancellationToken = default)
    {
        await context.Stores.AddAsync(store, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Store store, CancellationToken cancellationToken = default)
    {
        context.Stores.Update(store);
        await context.SaveChangesAsync(cancellationToken);
    }
}
