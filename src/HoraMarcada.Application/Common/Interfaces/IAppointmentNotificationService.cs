using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Common.Interfaces;

public interface IAppointmentNotificationService
{
    Task DispatchBookingNotificationsAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default);

    Task DispatchCancellationNotificationsAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default);
}
