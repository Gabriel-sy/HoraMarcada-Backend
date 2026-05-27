using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Infrastructure.Notifications;

public enum NotificationJobType
{
    BookingConfirmationToCustomer,
    BookingNotificationToProfessional,
    BookingNotificationToStore,
    CancellationNoticeToCustomer,
    CancellationNotificationToProfessional,
    CancellationNotificationToStore
}

public record NotificationJob(
    NotificationJobType Type,
    Appointment Appointment,
    Store Store,
    Service Service,
    Professional Professional
);
