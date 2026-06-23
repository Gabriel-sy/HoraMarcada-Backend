using HoraMarcada.Application.Common;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Application.UseCases.Appointments;

public class CancelAppointmentByTokenUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreRepository storeRepository,
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    IAppointmentNotificationService appointmentNotificationService)
{
    public async Task<string> ExecuteAsync(string cancelToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cancelToken))
            throw new DomainValidationException("Cancel token is required.");

        var appointment = await appointmentRepository.GetByCancelTokenAsync(cancelToken, cancellationToken);
        if (appointment == null)
        {
            throw new NotFoundException("Appointment with the provided cancel token not found.");
        }

        if (appointment.CancelTokenUsed)
        {
            throw new ConflictException("The cancel token has already been used.");
        }

        appointment.Cancel();
        appointment.MarkCancelTokenUsed();

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);

        var store = await storeRepository.GetByIdAsync(appointment.StoreId, cancellationToken);
        if (store == null)
        {
            throw new NotFoundException("Store not found for this appointment.");
        }

        var service = await serviceRepository.GetByIdAsync(appointment.ServiceId, cancellationToken);
        var professional = await professionalRepository.GetByIdAsync(appointment.ProfessionalId, cancellationToken);

        if (service != null && professional != null)
        {
            await appointmentNotificationService.DispatchCancellationNotificationsAsync(appointment, store, service, professional, cancellationToken);
        }

        return store.Slug.Value;
    }
}
