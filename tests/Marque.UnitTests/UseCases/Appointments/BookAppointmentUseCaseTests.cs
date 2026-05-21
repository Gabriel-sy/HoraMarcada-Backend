using Moq;
using Xunit;
using Marque.Application.UseCases.Appointments;
using Marque.Application.UseCases.Availability;
using Marque.Application.Repositories;
using Marque.Application.Common;
using Marque.Application.Models;
using Marque.Application.Exceptions;
using Marque.Domain.Entities;
using Marque.Domain.Exceptions;

namespace Marque.UnitTests.UseCases.Appointments;

public class BookAppointmentUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly GetAvailableSlotsUseCase _getAvailableSlotsUseCase;
    private readonly BookAppointmentUseCase _useCase;

    public BookAppointmentUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _timeProviderMock = new Mock<ITimeProvider>();

        _getAvailableSlotsUseCase = new GetAvailableSlotsUseCase(
            _storeRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _appointmentRepositoryMock.Object
        );

        _useCase = new BookAppointmentUseCase(
            _appointmentRepositoryMock.Object,
            _storeRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _getAvailableSlotsUseCase,
            _timeProviderMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenStartTimeIsInThePast_ShouldThrowDomainValidationException()
    {
        // Arrange
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

        // Act & Assert
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
        // Arrange
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

        // Act & Assert
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
        // Arrange
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

        var store = Store.Create(input.StoreId, "Store", "store", "+5511999999999", active: false);
        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(input.StoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        // Act & Assert
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
        // Arrange
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

        var store = Store.Create(input.StoreId, "Store", "store", "+5511999999999");
        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(input.StoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(input.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act & Assert
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
        // Arrange
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

        var store = Store.Create(storeId, "Store", "store", "+5511999999999");
        var service = Service.Create(storeId, "Service", 30); // Note: professionalId is NOT assigned
        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);

        // Act & Assert
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
        // Arrange
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

        var store = Store.Create(storeId, "Store", "store", "+5511999999999");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        // Not adding weekly hours for this day of week, so slot won't be available

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(input));

        _timeProviderMock.Verify(t => t.UtcNow, Times.Once);
        // Called once in BookAppointmentUseCase and once in GetAvailableSlotsUseCase
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Exactly(3));
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInputAndSlotAvailable_ShouldBookAppointment()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new BookAppointmentInput(
            StoreId: storeId,
            ProfessionalId: professionalId,
            ServiceId: serviceId,
            CustomerName: "Customer",
            CustomerWhatsApp: "+5511999999999",
            StartTime: new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3)) // 2026-05-21 is Thursday (4)
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(input.StartTime.AddMinutes(-5));

        var store = Store.Create(storeId, "Store", "store", "+5511999999999");
        var service = Service.Create(storeId, "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        // Setup working hours so 10:00 is available
        professional.AddWeeklyHours(4, new TimeOnly(9, 0), new TimeOnly(12, 0), intervalMinutes: 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>()); // No conflicting appointments

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(storeId, result.StoreId);
        Assert.Equal(professionalId, result.ProfessionalId);
        Assert.Equal(serviceId, result.ServiceId);
        Assert.Equal(input.CustomerName, result.CustomerName);
        Assert.Equal(input.CustomerWhatsApp, result.CustomerWhatsApp.Value);
        Assert.Equal(input.StartTime, result.StartTime);
        Assert.Equal(input.StartTime.AddMinutes(30), result.EndTime);
        Assert.NotNull(result.CancelToken);

        _timeProviderMock.Verify(t => t.UtcNow, Times.Once);
        // Called once in BookAppointmentUseCase and once in GetAvailableSlotsUseCase
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
            a.StartTime == input.StartTime &&
            a.EndTime == input.StartTime.AddMinutes(30) &&
            a.CancelToken == result.CancelToken
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
