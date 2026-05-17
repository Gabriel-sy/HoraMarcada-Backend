using Marque.Application.Repositories;
using Marque.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marque.Infrastructure.Persistence.Repositories;

internal sealed class EfProfessionalRepository(MarqueDbContext context) : IProfessionalRepository
{
    public async Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Professionals
            .Include(p => p.WeeklySchedules)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Professional>> ListByStoreIdAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        return await context.Professionals
            .Where(p => p.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Professional professional, CancellationToken cancellationToken = default)
    {
        await context.Professionals.AddAsync(professional, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
