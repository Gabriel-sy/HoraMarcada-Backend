using Moq;
using Xunit;
using HoraMarcada.Infrastructure.Notifications;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.Infrastructure.Notifications;

public class QueueNotificationServiceDecoratorTests
{
    private readonly Mock<INotificationQueue> _queueMock;
    private readonly QueueNotificationServiceDecorator _decorator;

    public QueueNotificationServiceDecoratorTests()
    {
        _queueMock = new Mock<INotificationQueue>();
        _decorator = new QueueNotificationServiceDecorator(_queueMock.Object);
    }

    [Fact]
    public async Task SendBookingConfirmationToCustomerAsync_ShouldQueueJob()
    {
        var store = Store.Create(Guid.NewGuid(), "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(store.Id, "Haircut", 30);
        var professional = Professional.Create(store.Id, "Jane Doe", email: "jane@example.com");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30)
        );

        await _decorator.SendBookingConfirmationToCustomerAsync(appointment, store, service, professional);

        _queueMock.Verify(q => q.QueueNotification(It.Is<NotificationJob>(j =>
            j.Type == NotificationJobType.BookingConfirmationToCustomer &&
            j.Appointment == appointment &&
            j.Store == store &&
            j.Service == service &&
            j.Professional == professional
        )), Times.Once);
    }

    [Fact]
    public async Task SendBookingNotificationToProfessionalAsync_ShouldQueueJob()
    {
        var store = Store.Create(Guid.NewGuid(), "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(store.Id, "Haircut", 30);
        var professional = Professional.Create(store.Id, "Jane Doe", email: "jane@example.com");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30)
        );

        await _decorator.SendBookingNotificationToProfessionalAsync(appointment, store, service, professional);

        _queueMock.Verify(q => q.QueueNotification(It.Is<NotificationJob>(j =>
            j.Type == NotificationJobType.BookingNotificationToProfessional &&
            j.Appointment == appointment &&
            j.Store == store &&
            j.Service == service &&
            j.Professional == professional
        )), Times.Once);
    }

    [Fact]
    public async Task SendBookingNotificationToStoreAsync_ShouldQueueJob()
    {
        var store = Store.Create(Guid.NewGuid(), "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(store.Id, "Haircut", 30);
        var professional = Professional.Create(store.Id, "Jane Doe", email: "jane@example.com");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30)
        );

        await _decorator.SendBookingNotificationToStoreAsync(appointment, store, service, professional);

        _queueMock.Verify(q => q.QueueNotification(It.Is<NotificationJob>(j =>
            j.Type == NotificationJobType.BookingNotificationToStore &&
            j.Appointment == appointment &&
            j.Store == store &&
            j.Service == service &&
            j.Professional == professional
        )), Times.Once);
    }

    [Fact]
    public async Task SendCancellationNoticeToCustomerAsync_ShouldQueueJob()
    {
        var store = Store.Create(Guid.NewGuid(), "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(store.Id, "Haircut", 30);
        var professional = Professional.Create(store.Id, "Jane Doe", email: "jane@example.com");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30)
        );

        await _decorator.SendCancellationNoticeToCustomerAsync(appointment, store, service, professional);

        _queueMock.Verify(q => q.QueueNotification(It.Is<NotificationJob>(j =>
            j.Type == NotificationJobType.CancellationNoticeToCustomer &&
            j.Appointment == appointment &&
            j.Store == store &&
            j.Service == service &&
            j.Professional == professional
        )), Times.Once);
    }

    [Fact]
    public async Task SendCancellationNotificationToProfessionalAsync_ShouldQueueJob()
    {
        var store = Store.Create(Guid.NewGuid(), "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(store.Id, "Haircut", 30);
        var professional = Professional.Create(store.Id, "Jane Doe", email: "jane@example.com");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30)
        );

        await _decorator.SendCancellationNotificationToProfessionalAsync(appointment, store, service, professional);

        _queueMock.Verify(q => q.QueueNotification(It.Is<NotificationJob>(j =>
            j.Type == NotificationJobType.CancellationNotificationToProfessional &&
            j.Appointment == appointment &&
            j.Store == store &&
            j.Service == service &&
            j.Professional == professional
        )), Times.Once);
    }

    [Fact]
    public async Task SendCancellationNotificationToStoreAsync_ShouldQueueJob()
    {
        var store = Store.Create(Guid.NewGuid(), "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(store.Id, "Haircut", 30);
        var professional = Professional.Create(store.Id, "Jane Doe", email: "jane@example.com");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30)
        );

        await _decorator.SendCancellationNotificationToStoreAsync(appointment, store, service, professional);

        _queueMock.Verify(q => q.QueueNotification(It.Is<NotificationJob>(j =>
            j.Type == NotificationJobType.CancellationNotificationToStore &&
            j.Appointment == appointment &&
            j.Store == store &&
            j.Service == service &&
            j.Professional == professional
        )), Times.Once);
    }
}
