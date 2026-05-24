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
    INotificationService notificationService)
{
    public async Task ExecuteAsync(string cancelToken, CancellationToken cancellationToken = default)
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
        var service = await serviceRepository.GetByIdAsync(appointment.ServiceId, cancellationToken);
        var professional = await professionalRepository.GetByIdAsync(appointment.ProfessionalId, cancellationToken);

        if (store != null && service != null && professional != null)
        {
            await notificationService.SendCancellationNoticeToCustomerAsync(appointment, store, service, professional, cancellationToken);
            await notificationService.SendCancellationNotificationToStoreAsync(appointment, store, service, professional, cancellationToken);

            if (professional.Email != null)
            {
                await notificationService.SendCancellationNotificationToProfessionalAsync(appointment, store, service, professional, cancellationToken);
            }
        }
    }
}
