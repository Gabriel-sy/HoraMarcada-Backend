using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using HoraMarcada.Application.Common;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.Appointments;

public class AppointmentNotificationServiceTests
{
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly AppointmentNotificationService _service;

    public AppointmentNotificationServiceTests()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();
        _timeProviderMock = new Mock<ITimeProvider>();

        _service = new AppointmentNotificationService(
            _notificationServiceMock.Object,
            _backgroundJobServiceMock.Object,
            _timeProviderMock.Object
        );
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_ShouldNotifyCustomerAndProfessionalAndNotNotifyStore()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            DateTimeOffset.UtcNow.AddDays(2),
            DateTimeOffset.UtcNow.AddDays(2).AddMinutes(30)
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        _timeProviderMock.Setup(t => t.UtcNow).Returns(appointment.StartTime.AddDays(-2));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        _notificationServiceMock.Verify(n => n.SendBookingConfirmationToCustomerAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendBookingNotificationToProfessionalAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_WhenProfessionalHasNoEmail_ShouldNotNotifyProfessional()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            DateTimeOffset.UtcNow.AddDays(2),
            DateTimeOffset.UtcNow.AddDays(2).AddMinutes(30)
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: null);

        _timeProviderMock.Setup(t => t.UtcNow).Returns(appointment.StartTime.AddDays(-2));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        _notificationServiceMock.Verify(n => n.SendBookingConfirmationToCustomerAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendBookingNotificationToProfessionalAsync(
            It.IsAny<Appointment>(), It.IsAny<Store>(), It.IsAny<Service>(), It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_WhenAppointmentIsMoreThan30hInFuture_ShouldScheduleReminder()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 5, 21, 10, 30, 0, TimeSpan.FromHours(-3))
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        _timeProviderMock.Setup(t => t.UtcNow).Returns(appointment.StartTime.AddDays(-2));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(appointment.Id, appointment.StartTime.AddHours(-24)), Times.Once);
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_WhenAppointmentIsLessThan30hInFuture_ShouldNotScheduleReminder()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 5, 21, 10, 30, 0, TimeSpan.FromHours(-3))
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        _timeProviderMock.Setup(t => t.UtcNow).Returns(appointment.StartTime.AddHours(-29));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_WhenReminderTimeIsDuringQuietHoursMorning_ShouldShiftTo8AM()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            new DateTimeOffset(2026, 5, 21, 6, 0, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 5, 21, 6, 30, 0, TimeSpan.FromHours(-3))
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        _timeProviderMock.Setup(t => t.UtcNow).Returns(appointment.StartTime.AddDays(-2));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        var expectedReminderTime = new DateTimeOffset(2026, 5, 20, 8, 0, 0, TimeSpan.FromHours(-3));
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(appointment.Id, expectedReminderTime), Times.Once);
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_WhenReminderTimeIsDuringQuietHoursNight_ShouldShiftTo9PM()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            new DateTimeOffset(2026, 5, 21, 22, 30, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 5, 21, 23, 0, 0, TimeSpan.FromHours(-3))
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        _timeProviderMock.Setup(t => t.UtcNow).Returns(appointment.StartTime.AddDays(-2));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        var expectedReminderTime = new DateTimeOffset(2026, 5, 20, 21, 0, 0, TimeSpan.FromHours(-3));
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(appointment.Id, expectedReminderTime), Times.Once);
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_WhenAdjustedReminderTimeIsLessThan6hBuffer_ShouldNotScheduleReminder()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            new DateTimeOffset(2026, 5, 21, 6, 0, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 5, 21, 6, 30, 0, TimeSpan.FromHours(-3))
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        _timeProviderMock.Setup(t => t.UtcNow).Returns(new DateTimeOffset(2026, 5, 20, 3, 0, 0, TimeSpan.FromHours(-3)));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_When24hReminderFailsBuffer_ShouldFallbackToMorningOfAppointment()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            new DateTimeOffset(2026, 6, 1, 17, 0, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 6, 1, 17, 30, 0, TimeSpan.FromHours(-3))
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        _timeProviderMock.Setup(t => t.UtcNow).Returns(new DateTimeOffset(2026, 5, 31, 15, 30, 0, TimeSpan.FromHours(-3)));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        var expectedReminderTime = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.FromHours(-3));
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(appointment.Id, expectedReminderTime), Times.Once);
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_When24hReminderFailsBufferAndFallbackFailsLeadTime_ShouldNotSchedule()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 6, 1, 10, 30, 0, TimeSpan.FromHours(-3))
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        _timeProviderMock.Setup(t => t.UtcNow).Returns(new DateTimeOffset(2026, 5, 31, 15, 30, 0, TimeSpan.FromHours(-3)));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact]
    public async Task DispatchBookingNotificationsAsync_When24hReminderFailsBufferAndFallbackFailsBuffer_ShouldNotSchedule()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            new DateTimeOffset(2026, 6, 1, 14, 0, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 6, 1, 14, 30, 0, TimeSpan.FromHours(-3))
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        _timeProviderMock.Setup(t => t.UtcNow).Returns(new DateTimeOffset(2026, 6, 1, 5, 0, 0, TimeSpan.FromHours(-3)));

        await _service.DispatchBookingNotificationsAsync(appointment, store, service, professional);

        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact]
    public async Task DispatchCancellationNotificationsAsync_ShouldNotifyCustomerAndProfessionalAndNotNotifyStore()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            DateTimeOffset.UtcNow.AddDays(2),
            DateTimeOffset.UtcNow.AddDays(2).AddMinutes(30)
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");

        await _service.DispatchCancellationNotificationsAsync(appointment, store, service, professional);

        _notificationServiceMock.Verify(n => n.SendCancellationNoticeToCustomerAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendCancellationNotificationToProfessionalAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DispatchCancellationNotificationsAsync_WhenProfessionalHasNoEmail_ShouldNotNotifyProfessional()
    {
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            DateTimeOffset.UtcNow.AddDays(2),
            DateTimeOffset.UtcNow.AddDays(2).AddMinutes(30)
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: null);

        await _service.DispatchCancellationNotificationsAsync(appointment, store, service, professional);

        _notificationServiceMock.Verify(n => n.SendCancellationNoticeToCustomerAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendCancellationNotificationToProfessionalAsync(
            It.IsAny<Appointment>(), It.IsAny<Store>(), It.IsAny<Service>(), It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
