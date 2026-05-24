using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Appointments;

public class CompleteAppointmentUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task ExecuteAsync(Guid userId, Guid appointmentId, CompleteAppointmentInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, input.StoreId, cancellationToken);

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null || appointment.StoreId != input.StoreId)
        {
            throw new NotFoundException($"Appointment with ID {appointmentId} not found in store {input.StoreId}.");
        }

        appointment.Complete();
        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}
