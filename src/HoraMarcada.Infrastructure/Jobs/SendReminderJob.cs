using System;
using System.Threading;
using System.Threading.Tasks;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Enums;
using HoraMarcada.Infrastructure.Notifications;
using Microsoft.Extensions.Logging;

namespace HoraMarcada.Infrastructure.Jobs;

public class SendReminderJob(
    IAppointmentRepository appointmentRepository,
    IStoreRepository storeRepository,
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    ResendEmailNotificationService emailNotificationService,
    ILogger<SendReminderJob> logger)
{
    public async Task ExecuteAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null)
        {
            logger.LogWarning("Appointment {AppointmentId} not found for 24h reminder.", appointmentId);
            return;
        }

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            logger.LogInformation("Appointment {AppointmentId} is in status {Status}. Skipping 24h reminder.", appointmentId, appointment.Status);
            return;
        }

        var store = await storeRepository.GetByIdAsync(appointment.StoreId, cancellationToken);
        var service = await serviceRepository.GetByIdAsync(appointment.ServiceId, cancellationToken);
        var professional = await professionalRepository.GetByIdAsync(appointment.ProfessionalId, cancellationToken);

        if (store == null || service == null || professional == null)
        {
            logger.LogError("Could not fetch details for appointment {AppointmentId} to send 24h reminder.", appointmentId);
            return;
        }

        await emailNotificationService.SendBookingReminder24hAsync(appointment, store, service, professional, cancellationToken);
    }
}
