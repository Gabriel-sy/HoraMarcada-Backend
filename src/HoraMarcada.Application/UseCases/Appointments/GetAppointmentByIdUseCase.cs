using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.UseCases.Appointments;

public class GetAppointmentByIdUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<Appointment> ExecuteAsync(Guid userId, Guid appointmentId, Guid storeId, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null || appointment.StoreId != storeId)
        {
            throw new NotFoundException($"Appointment {appointmentId} not found in store {storeId}.");
        }

        return appointment;
    }
}
