using Moq;
using Xunit;
using HoraMarcada.Infrastructure.Notifications;
using HoraMarcada.Infrastructure.Notifications.Queue;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp;
using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Client;
using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Simulation;

namespace HoraMarcada.UnitTests.Infrastructure.Notifications;

public class QueueNotificationServiceDecoratorTests
{
    private readonly Mock<INotificationQueue> _queueMock;
    private readonly Mock<IAppConfiguration> _configMock;
    private readonly QueueNotificationServiceDecorator _decorator;

    public QueueNotificationServiceDecoratorTests()
    {
        _queueMock = new Mock<INotificationQueue>();
        _configMock = new Mock<IAppConfiguration>();
        _configMock.Setup(c => c.NotificationsEnabled).Returns(true);
        var config = new ConfigurationBuilder().Build();
        var dispatcher = new WahaWhatsAppNotificationService(
            new Mock<IWahaClient>().Object,
            new Mock<IWhatsAppHumanBehavior>().Object,
            config,
            new Mock<ILogger<WahaWhatsAppNotificationService>>().Object);
        _decorator = new QueueNotificationServiceDecorator(_queueMock.Object, _configMock.Object, dispatcher);
    }

    [Fact]
    public async Task SendBookingConfirmationToCustomerAsync_ShouldQueueJob()
    {
        var store = Store.Create(Guid.NewGuid(), "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(store.Id, "Haircut", 30);
        var professional = Professional.Create(store.Id, "Jane Doe");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
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
        var professional = Professional.Create(store.Id, "Jane Doe");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
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
    public async Task SendCancellationNoticeToCustomerAsync_ShouldQueueJob()
    {
        var store = Store.Create(Guid.NewGuid(), "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(store.Id, "Haircut", 30);
        var professional = Professional.Create(store.Id, "Jane Doe");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
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
        var professional = Professional.Create(store.Id, "Jane Doe");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
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
    public async Task SendBookingConfirmationToCustomerAsync_WhenNotificationsDisabled_ShouldNotQueueJob()
    {
        _configMock.Setup(c => c.NotificationsEnabled).Returns(false);
        var store = Store.Create(Guid.NewGuid(), "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(store.Id, "Haircut", 30);
        var professional = Professional.Create(store.Id, "Jane Doe");
        var appointment = Appointment.Create(
            store.Id,
            professional.Id,
            service.Id,
            "John Doe",
            "+5511988888888",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30)
        );

        await _decorator.SendBookingConfirmationToCustomerAsync(appointment, store, service, professional);

        _queueMock.Verify(q => q.QueueNotification(It.IsAny<NotificationJob>()), Times.Never);
    }
}
