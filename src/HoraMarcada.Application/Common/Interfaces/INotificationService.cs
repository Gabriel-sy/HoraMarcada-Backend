using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendBookingConfirmationToCustomerAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default);

    Task SendBookingNotificationToProfessionalAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default);

    Task SendCancellationNoticeToCustomerAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default);

    Task SendCancellationNotificationToProfessionalAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default);

    Task SendBookingReminder24hAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default);

    Task SendDailySummaryToProfessionalAsync(
        Professional professional,
        Store store,
        IReadOnlyList<Appointment> appointments,
        IReadOnlyList<Service> services,
        CancellationToken cancellationToken = default);
}
