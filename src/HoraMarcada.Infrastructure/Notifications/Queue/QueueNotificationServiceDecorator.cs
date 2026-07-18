using HoraMarcada.Application.Common.Interfaces;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp;

namespace HoraMarcada.Infrastructure.Notifications.Queue;

public class QueueNotificationServiceDecorator(
    INotificationQueue queue,
    IAppConfiguration appConfiguration,
    WahaWhatsAppNotificationService dispatcher) : INotificationService
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

    public Task SendDailySummaryToProfessionalAsync(
        Professional professional,
        Store store,
        IReadOnlyList<Appointment> appointments,
        IReadOnlyList<Service> services,
        CancellationToken cancellationToken = default)
    {
        if (!appConfiguration.NotificationsEnabled)
        {
            return Task.CompletedTask;
        }

        return dispatcher.SendDailySummaryToProfessionalAsync(
            professional,
            store,
            appointments,
            services,
            cancellationToken);
    }
}
