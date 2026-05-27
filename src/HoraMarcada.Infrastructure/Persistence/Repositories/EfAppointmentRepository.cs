using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoraMarcada.Infrastructure.Persistence.Repositories;

internal sealed class EfAppointmentRepository(HoraMarcadaDbContext context) : IAppointmentRepository
{
    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await context.Appointments.AddAsync(appointment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> ListByStoreAsync(
        Guid storeId,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? professionalId = null,
        CancellationToken cancellationToken = default)
    {
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();

        var query = context.Appointments
            .Where(a => a.StoreId == storeId && a.StartTime >= startUtc && a.StartTime <= endUtc);

        if (professionalId.HasValue)
        {
            query = query.Where(a => a.ProfessionalId == professionalId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> ListByProfessionalAndDateAsync(
        Guid professionalId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, date.Offset).ToUniversalTime();
        var endOfDay = startOfDay.AddDays(1);

        return await context.Appointments
            .Where(a => a.ProfessionalId == professionalId &&
                        a.StartTime >= startOfDay &&
                        a.StartTime < endOfDay)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetByCancelTokenAsync(
        string cancelToken,
        CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .FirstOrDefaultAsync(a => a.CancelToken == cancelToken, cancellationToken);
    }

    public async Task UpdateAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default)
    {
        context.Appointments.Update(appointment);
        await context.SaveChangesAsync(cancellationToken);
    }
}
