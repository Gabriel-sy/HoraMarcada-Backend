using Marque.Application.Repositories;
using Marque.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marque.Infrastructure.Persistence.Repositories;

internal sealed class EfAppointmentRepository(MarqueDbContext context) : IAppointmentRepository
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
}
