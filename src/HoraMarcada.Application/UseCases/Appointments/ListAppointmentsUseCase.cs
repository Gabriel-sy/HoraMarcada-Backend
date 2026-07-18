using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Appointments;

public class ListAppointmentsUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<IReadOnlyList<AppointmentDto>> ExecuteAsync(
        Guid userId,
        ListAppointmentsInput input,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, input.StoreId, cancellationToken);

        var list = await appointmentRepository.ListByStoreAsync(input.StoreId, input.Start, input.End, input.ProfessionalId, cancellationToken);
        return list.OrderBy(a => a.StartTime).Select(AppointmentDto.FromDomain).ToList();
    }
}
