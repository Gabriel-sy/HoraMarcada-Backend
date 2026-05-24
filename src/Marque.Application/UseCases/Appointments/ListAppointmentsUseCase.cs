using Marque.Application.Models;
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
        ListAppointmentsInput input,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, input.StoreId, cancellationToken);

        var list = await appointmentRepository.ListByStoreAsync(input.StoreId, input.Start, input.End, input.ProfessionalId, cancellationToken);
        return list.OrderBy(a => a.StartTime).ToList();
    }
}
