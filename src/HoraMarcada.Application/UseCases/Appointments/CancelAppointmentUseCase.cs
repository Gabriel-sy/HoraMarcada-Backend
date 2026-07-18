using HoraMarcada.Application.Common.Interfaces;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Appointments;

public class CancelAppointmentUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreRepository storeRepository,
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy,
    IAppointmentNotificationService appointmentNotificationService)
{
    public async Task ExecuteAsync(Guid userId, Guid appointmentId, CancelAppointmentInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, input.StoreId, cancellationToken);

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null || appointment.StoreId != input.StoreId)
        {
            throw new NotFoundException($"Appointment with ID {appointmentId} not found in store {input.StoreId}.");
        }

        appointment.Cancel();
        await appointmentRepository.UpdateAsync(appointment, cancellationToken);

        var store = await storeRepository.GetByIdAsync(appointment.StoreId, cancellationToken);
        var service = await serviceRepository.GetByIdAsync(appointment.ServiceId, cancellationToken);
        var professional = await professionalRepository.GetByIdAsync(appointment.ProfessionalId, cancellationToken);

        if (store != null && service != null && professional != null)
        {
            await appointmentNotificationService.DispatchCancellationNotificationsAsync(appointment, store, service, professional, cancellationToken);
        }
    }
}
