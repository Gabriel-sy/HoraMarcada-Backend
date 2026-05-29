using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoraMarcada.Infrastructure.Persistence.Repositories;

internal sealed class EfTimeBlockRepository(HoraMarcadaDbContext context) : ITimeBlockRepository
{
    public async Task<TimeBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TimeBlocks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(TimeBlock timeBlock, CancellationToken cancellationToken = default)
    {
        await context.TimeBlocks.AddAsync(timeBlock, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TimeBlock timeBlock, CancellationToken cancellationToken = default)
    {
        context.TimeBlocks.Remove(timeBlock);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimeBlock>> ListByProfessionalAndDateAsync(
        Guid professionalId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, date.Offset).ToUniversalTime();
        var endOfDay = startOfDay.AddDays(1);

        return await context.TimeBlocks
            .Where(t => t.ProfessionalId == professionalId &&
                        t.StartTime < endOfDay &&
                        t.EndTime > startOfDay)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimeBlock>> ListByProfessionalAndRangeAsync(
        Guid professionalId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();

        return await context.TimeBlocks
            .Where(t => t.ProfessionalId == professionalId &&
                        t.StartTime < endUtc &&
                        t.EndTime > startUtc)
            .ToListAsync(cancellationToken);
    }
}
