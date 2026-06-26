using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Infrastructure.Notifications.Queue;

public enum NotificationJobType
{
    BookingConfirmationToCustomer,
    BookingNotificationToProfessional,
    CancellationNoticeToCustomer,
    CancellationNotificationToProfessional,
    BookingReminder24h
}

public record NotificationJob(
    NotificationJobType Type,
    Appointment Appointment,
    Store Store,
    Service Service,
    Professional Professional
);
