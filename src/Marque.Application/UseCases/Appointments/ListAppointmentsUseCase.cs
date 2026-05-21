using Marque.Application.Policies;
using Marque.Application.Repositories;
using Marque.Domain.Entities;

namespace Marque.Application.UseCases.Appointments;

public class ListAppointmentsUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<IReadOnlyList<Appointment>> ExecuteAsync(
        Guid userId,
        Guid storeId,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? professionalId = null,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var list = await appointmentRepository.ListByStoreAsync(storeId, start, end, professionalId, cancellationToken);
        return list.OrderBy(a => a.StartTime).ToList();
    }
}
