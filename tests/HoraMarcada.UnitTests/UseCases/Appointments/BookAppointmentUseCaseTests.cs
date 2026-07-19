using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Common.Interfaces;
using HoraMarcada.Application.Common.Services;
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
    private readonly Mock<IAvailabilityService> _availabilityServiceMock;
    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly Mock<IAppointmentNotificationService> _appointmentNotificationServiceMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly CancelTokenGenerator _cancelTokenGenerator;
    private readonly BookAppointmentUseCase _useCase;

    public BookAppointmentUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _availabilityServiceMock = new Mock<IAvailabilityService>();
        _timeProviderMock = new Mock<ITimeProvider>();
        _appointmentNotificationServiceMock = new Mock<IAppointmentNotificationService>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _cancelTokenGenerator = new CancelTokenGenerator();

        _availabilityServiceMock
            .Setup(s => s.GetAvailableSlotsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DateTimeOffset>());

        _customerRepositoryMock
            .Setup(r => r.GetByWhatsAppAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        _appointmentRepositoryMock
            .Setup(r => r.GetByCancelTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        _useCase = new BookAppointmentUseCase(
            _appointmentRepositoryMock.Object,
            _storeRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _availabilityServiceMock.Object,
            _appointmentNotificationServiceMock.Object,
            _timeProviderMock.Object,
            _customerRepositoryMock.Object,
            _cancelTokenGenerator
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
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);
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
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);

        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(input));

        _timeProviderMock.Verify(t => t.UtcNow, Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _availabilityServiceMock.Verify(s => s.GetAvailableSlotsAsync(
            storeId, serviceId, input.StartTime, professionalId, It.IsAny<CancellationToken>()), Times.Once);
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
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);

        _availabilityServiceMock
            .Setup(s => s.GetAvailableSlotsAsync(storeId, serviceId, input.StartTime, professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DateTimeOffset> { input.StartTime });

        var result = await _useCase.ExecuteAsync(input);

        Assert.NotNull(result);
        Assert.Equal(storeId, result.StoreId);
        Assert.Equal(professionalId, result.ProfessionalId);
        Assert.Equal(serviceId, result.ServiceId);
        Assert.Equal(input.CustomerName, result.CustomerName);
        Assert.Equal(input.CustomerWhatsApp, result.CustomerWhatsApp);
        Assert.Equal(input.StartTime, result.StartTime);
        Assert.Equal(input.StartTime.AddMinutes(30), result.EndTime);
        Assert.NotNull(result.CancelToken);
        Assert.Equal(5, result.CancelToken.Length);

        _timeProviderMock.Verify(t => t.UtcNow, Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _availabilityServiceMock.Verify(s => s.GetAvailableSlotsAsync(
            storeId, serviceId, input.StartTime, professionalId, It.IsAny<CancellationToken>()), Times.Once);

        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.Is<Appointment>(a =>
            a.Id == result.Id &&
            a.StoreId == storeId &&
            a.ProfessionalId == professionalId &&
            a.ServiceId == serviceId &&
            a.CustomerName == input.CustomerName &&
            a.CustomerWhatsApp.Value == input.CustomerWhatsApp &&
            a.StartTime == input.StartTime &&
            a.EndTime == input.StartTime.AddMinutes(30) &&
            a.CancelToken == result.CancelToken
        ), It.IsAny<CancellationToken>()), Times.Once);

        _appointmentNotificationServiceMock.Verify(n => n.DispatchBookingNotificationsAsync(
            It.Is<Appointment>(a => a.Id == result.Id),
            store,
            service,
            professional,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
