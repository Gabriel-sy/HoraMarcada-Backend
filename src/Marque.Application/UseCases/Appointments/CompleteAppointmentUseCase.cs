using Marque.Application.Exceptions;
using Marque.Application.Policies;
using Marque.Application.Repositories;

namespace Marque.Application.UseCases.Appointments;

public class CompleteAppointmentUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task ExecuteAsync(Guid userId, Guid storeId, Guid appointmentId, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null || appointment.StoreId != storeId)
        {
            throw new NotFoundException($"Appointment with ID {appointmentId} not found in store {storeId}.");
        }

        appointment.Complete();
        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}
