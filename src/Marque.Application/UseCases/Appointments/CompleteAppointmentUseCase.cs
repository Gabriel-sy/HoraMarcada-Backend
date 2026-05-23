using Marque.Application.Exceptions;
using Marque.Application.Models;
using Marque.Application.Policies;
using Marque.Application.Repositories;

namespace Marque.Application.UseCases.Appointments;

public class CompleteAppointmentUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task ExecuteAsync(Guid appointmentId, CompleteAppointmentInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, cancellationToken);

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null || appointment.StoreId != input.StoreId)
        {
            throw new NotFoundException($"Appointment with ID {appointmentId} not found in store {input.StoreId}.");
        }

        appointment.Complete();
        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}
