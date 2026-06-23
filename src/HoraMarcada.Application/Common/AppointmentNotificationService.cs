using System;
using System.Threading;
using System.Threading.Tasks;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Common;

public class AppointmentNotificationService(
    INotificationService notificationService,
    IBackgroundJobService backgroundJobService,
    ITimeProvider timeProvider) : IAppointmentNotificationService
{
    public async Task DispatchBookingNotificationsAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        await notificationService.SendBookingConfirmationToCustomerAsync(appointment, store, service, professional, cancellationToken);

        if (professional.Email != null)
        {
            await notificationService.SendBookingNotificationToProfessionalAsync(appointment, store, service, professional, cancellationToken);
        }

        var localStartTime = appointment.StartTime.ToOffset(TimeSpan.FromHours(-3));
        var reminderTime = localStartTime.AddHours(-24);
        var localHour = reminderTime.Hour;

        if (localHour < 8)
        {
            reminderTime = new DateTimeOffset(
                reminderTime.Year, reminderTime.Month, reminderTime.Day,
                8, 0, 0, reminderTime.Offset);
        }
        else if (localHour >= 22)
        {
            reminderTime = new DateTimeOffset(
                reminderTime.Year, reminderTime.Month, reminderTime.Day,
                21, 0, 0, reminderTime.Offset);
        }

        if (reminderTime > timeProvider.UtcNow.AddHours(6))
        {
            backgroundJobService.ScheduleReminder(appointment.Id, reminderTime);
        }
        else
        {
            var morningReminderTime = new DateTimeOffset(
                localStartTime.Year, localStartTime.Month, localStartTime.Day,
                8, 0, 0, localStartTime.Offset);

            if (morningReminderTime > timeProvider.UtcNow.AddHours(6) &&
                morningReminderTime <= localStartTime.AddHours(-3))
            {
                backgroundJobService.ScheduleReminder(appointment.Id, morningReminderTime);
            }
        }
    }

    public async Task DispatchCancellationNotificationsAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        await notificationService.SendCancellationNoticeToCustomerAsync(appointment, store, service, professional, cancellationToken);

        if (professional.Email != null)
        {
            await notificationService.SendCancellationNotificationToProfessionalAsync(appointment, store, service, professional, cancellationToken);
        }
    }
}
