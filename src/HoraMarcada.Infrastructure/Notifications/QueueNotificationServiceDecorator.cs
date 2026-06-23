using HoraMarcada.Application.Common;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Infrastructure.Notifications;

public class QueueNotificationServiceDecorator(INotificationQueue queue, IAppConfiguration appConfiguration) : INotificationService
{
    public Task SendBookingConfirmationToCustomerAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        if (!appConfiguration.NotificationsEnabled)
        {
            return Task.CompletedTask;
        }

        queue.QueueNotification(new NotificationJob(
            NotificationJobType.BookingConfirmationToCustomer,
            appointment,
            store,
            service,
            professional
        ));
        return Task.CompletedTask;
    }

    public Task SendBookingNotificationToProfessionalAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        if (!appConfiguration.NotificationsEnabled)
        {
            return Task.CompletedTask;
        }

        queue.QueueNotification(new NotificationJob(
            NotificationJobType.BookingNotificationToProfessional,
            appointment,
            store,
            service,
            professional
        ));
        return Task.CompletedTask;
    }

    public Task SendCancellationNoticeToCustomerAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        if (!appConfiguration.NotificationsEnabled)
        {
            return Task.CompletedTask;
        }

        queue.QueueNotification(new NotificationJob(
            NotificationJobType.CancellationNoticeToCustomer,
            appointment,
            store,
            service,
            professional
        ));
        return Task.CompletedTask;
    }

    public Task SendCancellationNotificationToProfessionalAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        if (!appConfiguration.NotificationsEnabled)
        {
            return Task.CompletedTask;
        }

        queue.QueueNotification(new NotificationJob(
            NotificationJobType.CancellationNotificationToProfessional,
            appointment,
            store,
            service,
            professional
        ));
        return Task.CompletedTask;
    }

    public Task SendBookingReminder24hAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        if (!appConfiguration.NotificationsEnabled)
        {
            return Task.CompletedTask;
        }

        queue.QueueNotification(new NotificationJob(
            NotificationJobType.BookingReminder24h,
            appointment,
            store,
            service,
            professional
        ));
        return Task.CompletedTask;
    }
}
