using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.UseCases.Availability;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Common;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.UnitTests.UseCases.Appointments;

public class BookAppointmentUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<ITimeBlockRepository> _timeBlockRepositoryMock;
    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly Mock<IAppConfiguration> _appConfigurationMock;
    private readonly GetAvailableSlotsUseCase _getAvailableSlotsUseCase;
    private readonly BookAppointmentUseCase _useCase;

    public BookAppointmentUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _timeBlockRepositoryMock = new Mock<ITimeBlockRepository>();
        _timeProviderMock = new Mock<ITimeProvider>();
        _notificationServiceMock = new Mock<INotificationService>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();
        _appConfigurationMock = new Mock<IAppConfiguration>();

        _appConfigurationMock.Setup(c => c.MinimumSlotLeadTimeMinutes).Returns(0);

        _timeBlockRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimeBlock>());

        _getAvailableSlotsUseCase = new GetAvailableSlotsUseCase(
            _storeRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _timeBlockRepositoryMock.Object,
            _timeProviderMock.Object,
            _appConfigurationMock.Object
        );

        _useCase = new BookAppointmentUseCase(
            _appointmentRepositoryMock.Object,
            _storeRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _getAvailableSlotsUseCase,
            _notificationServiceMock.Object,
            _backgroundJobServiceMock.Object,
            _timeProviderMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenStartTimeIsInThePast_ShouldThrowDomainValidationException()
    {
        var input = new BookAppointmentInput(
            StoreId: Guid.NewGuid(),
            ProfessionalId: Guid.NewGuid(),
            ServiceId: Guid.NewGuid(),
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2000, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(1));

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(input));

        _timeProviderMock.Verify(t => t.UtcNow, Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreDoesNotExist_ShouldThrowNotFoundException()
    {
        var input = new BookAppointmentInput(
            StoreId: Guid.NewGuid(),
            ProfessionalId: Guid.NewGuid(),
            ServiceId: Guid.NewGuid(),
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(input.StoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(input));

        _timeProviderMock.Verify(t => t.UtcNow, Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreInactive_ShouldThrowConflictException()
    {
        var input = new BookAppointmentInput(
            StoreId: Guid.NewGuid(),
            ProfessionalId: Guid.NewGuid(),
            ServiceId: Guid.NewGuid(),
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        var store = Store.Create(input.StoreId, "Store", "store", "+5511999999999", "store@example.com", active: false);
        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(input.StoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(input));

        _timeProviderMock.Verify(t => t.UtcNow, Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceDoesNotExist_ShouldThrowNotFoundException()
    {
        var input = new BookAppointmentInput(
            StoreId: Guid.NewGuid(),
            ProfessionalId: Guid.NewGuid(),
            ServiceId: Guid.NewGuid(),
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        var store = Store.Create(input.StoreId, "Store", "store", "+5511999999999", "store@example.com");
        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(input.StoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(input.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(input));

        _timeProviderMock.Verify(t => t.UtcNow, Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(input.ServiceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalIsNotAssignedToService_ShouldThrowDomainValidationException()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(input));

        _timeProviderMock.Verify(t => t.UtcNow, Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTimeSlotNotAvailable_ShouldThrowConflictException()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);

        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(input));

        _timeProviderMock.Verify(t => t.UtcNow, Times.AtLeastOnce());
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Exactly(3));
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInputAndSlotAvailable_ShouldBookAppointment()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(4, new TimeOnly(9, 0), new TimeOnly(12, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        Assert.Equal(storeId, result.StoreId);
        Assert.Equal(professionalId, result.ProfessionalId);
        Assert.Equal(serviceId, result.ServiceId);
        Assert.Equal(input.CustomerName, result.CustomerName);
        Assert.Equal(input.CustomerWhatsApp, result.CustomerWhatsApp.Value);
        Assert.Equal(input.StartTime, result.StartTime);
        Assert.Equal(input.StartTime.AddMinutes(30), result.EndTime);
        Assert.NotNull(result.CancelToken);

        _timeProviderMock.Verify(t => t.UtcNow, Times.AtLeastOnce());
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Exactly(3));
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        
        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.Is<Appointment>(a => 
            a.Id == result.Id &&
            a.StoreId == storeId &&
            a.ProfessionalId == professionalId &&
            a.ServiceId == serviceId &&
            a.CustomerName == input.CustomerName &&
            a.CustomerWhatsApp.Value == input.CustomerWhatsApp &&
            a.CustomerEmail.Value == input.CustomerEmail &&
            a.StartTime == input.StartTime &&
            a.EndTime == input.StartTime.AddMinutes(30) &&
            a.CancelToken == result.CancelToken
        ), It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendBookingConfirmationToCustomerAsync(
            result, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendBookingNotificationToStoreAsync(
            result, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendBookingNotificationToProfessionalAsync(
            result, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);

        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAppointmentIsMoreThan30hInFuture_ShouldScheduleReminder()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddDays(-2));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(4, new TimeOnly(9, 0), new TimeOnly(12, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(result.Id, result.StartTime.AddHours(-24)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAppointmentIsLessThan30hInFuture_ShouldNotScheduleReminder()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddHours(-29));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(4, new TimeOnly(9, 0), new TimeOnly(12, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenReminderTimeIsDuringQuietHoursMorning_ShouldShiftTo8AM()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 6, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddDays(-2));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(4, new TimeOnly(6, 0), new TimeOnly(12, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        var expectedReminderTime = new DateTimeOffset(2026, 5, 20, 8, 0, 0, TimeSpan.FromHours(-3));
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(result.Id, expectedReminderTime), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenReminderTimeIsDuringQuietHoursNight_ShouldShiftTo9PM()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 22, 30, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddDays(-2));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(4, new TimeOnly(9, 0), new TimeOnly(23, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        var expectedReminderTime = new DateTimeOffset(2026, 5, 20, 21, 0, 0, TimeSpan.FromHours(-3));
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(result.Id, expectedReminderTime), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAdjustedReminderTimeIsLessThan6hBuffer_ShouldNotScheduleReminder()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 5, 21, 6, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(new DateTimeOffset(2026, 5, 20, 3, 0, 0, TimeSpan.FromHours(-3)));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(4, new TimeOnly(6, 0), new TimeOnly(12, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_When24hReminderFailsBuffer_ShouldFallbackToMorningOfAppointment()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 6, 1, 17, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(new DateTimeOffset(2026, 5, 31, 15, 30, 0, TimeSpan.FromHours(-3)));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(1, new TimeOnly(9, 0), new TimeOnly(18, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        var expectedReminderTime = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.FromHours(-3));
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(result.Id, expectedReminderTime), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_When24hReminderFailsBufferAndFallbackFailsLeadTime_ShouldNotSchedule()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(new DateTimeOffset(2026, 5, 31, 15, 30, 0, TimeSpan.FromHours(-3)));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(1, new TimeOnly(9, 0), new TimeOnly(18, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_When24hReminderFailsBufferAndFallbackFailsBuffer_ShouldNotSchedule()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            CustomerEmail: "customer@example.com",
            StartTime: new DateTimeOffset(2026, 6, 1, 14, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(new DateTimeOffset(2026, 6, 1, 5, 0, 0, TimeSpan.FromHours(-3)));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe", email: "professional@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(1, new TimeOnly(9, 0), new TimeOnly(18, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        _backgroundJobServiceMock.Verify(s => s.ScheduleReminder(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }
}
