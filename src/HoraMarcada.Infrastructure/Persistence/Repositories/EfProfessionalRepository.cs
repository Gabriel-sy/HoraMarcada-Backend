using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoraMarcada.Infrastructure.Persistence.Repositories;

internal sealed class EfProfessionalRepository(HoraMarcadaDbContext context) : IProfessionalRepository
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
            .Include(p => p.WeeklySchedules)
            .Where(p => p.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Professional professional, CancellationToken cancellationToken = default)
    {
        await context.Professionals.AddAsync(professional, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Professional professional, CancellationToken cancellationToken = default)
    {
        var entry = context.Entry(professional);
        if (entry.State == EntityState.Detached)
        {
            context.Professionals.Update(professional);
        }
        else
        {
            foreach (var weeklyHour in professional.WeeklySchedules)
            {
                context.Entry(weeklyHour).State = EntityState.Added;
            }
        }
        await context.SaveChangesAsync(cancellationToken);
    }
}
