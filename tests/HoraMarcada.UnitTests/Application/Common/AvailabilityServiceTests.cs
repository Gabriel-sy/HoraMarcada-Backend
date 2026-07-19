using Moq;
using Xunit;
using HoraMarcada.Application.Common.Interfaces;
using HoraMarcada.Application.Common.Services;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.UnitTests.Application.Common;

public class AvailabilityServiceTests
{
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<ITimeBlockRepository> _timeBlockRepositoryMock;
    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly Mock<IAppConfiguration> _appConfigurationMock;
    private readonly AvailabilityService _service;

    public AvailabilityServiceTests()
    {
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _timeBlockRepositoryMock = new Mock<ITimeBlockRepository>();
        _timeProviderMock = new Mock<ITimeProvider>();
        _appConfigurationMock = new Mock<IAppConfiguration>();

        _appConfigurationMock.Setup(c => c.MinimumSlotLeadTimeMinutes).Returns(0);

        _timeBlockRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimeBlock>());

        _service = new AvailabilityService(
            _storeRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _timeBlockRepositoryMock.Object,
            _timeProviderMock.Object,
            _appConfigurationMock.Object
        );
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenStoreDoesNotExist_ShouldThrowNotFoundException()
    {
        var storeId = Guid.NewGuid();
        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync((Store?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAvailableSlotsAsync(storeId, Guid.NewGuid(), DateTimeOffset.UtcNow));
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenServiceDoesNotExist_ShouldThrowNotFoundException()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var store = Store.Create(Guid.NewGuid(), "Store", "store", "+5511999999999", "store@example.com");
        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync((Service?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAvailableSlotsAsync(storeId, serviceId, DateTimeOffset.UtcNow));
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenServiceDoesNotBelongToStore_ShouldThrowNotFoundException()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var store = Store.Create(Guid.NewGuid(), "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(otherStoreId, Guid.NewGuid(), "Service", 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAvailableSlotsAsync(storeId, serviceId, DateTimeOffset.UtcNow));
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenServiceIsInactive_ShouldReturnEmptySlots()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var store = Store.Create(Guid.NewGuid(), "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30, active: false);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        var result = await _service.GetAvailableSlotsAsync(storeId, serviceId, DateTimeOffset.UtcNow);

        Assert.Empty(result);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenSpecificProfessionalRequestedAndDoesNotExist_ShouldThrowNotFoundException()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var store = Store.Create(Guid.NewGuid(), "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync((Professional?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAvailableSlotsAsync(storeId, serviceId, DateTimeOffset.UtcNow, professionalId));
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenSpecificProfessionalRequestedButNotAssignedToService_ShouldThrowDomainValidationException()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var store = Store.Create(Guid.NewGuid(), "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);
        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);

        await Assert.ThrowsAsync<DomainValidationException>(() => _service.GetAvailableSlotsAsync(storeId, serviceId, DateTimeOffset.UtcNow, professionalId));
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenValidInput_ShouldGenerateSlotsAndFilterByLunchAndAppointments()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var date = new DateTimeOffset(2026, 5, 21, 0, 0, 0, TimeSpan.FromHours(-3));

        var store = Store.Create(Guid.NewGuid(), "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe");
        var profIdField = typeof(Professional).GetProperty("Id");
        profIdField?.SetValue(professional, professionalId);

        professional.AddWeeklyHours(
            dayOfWeek: 4,
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(12, 0),
            lunchStart: new TimeOnly(10, 30),
            lunchEnd: new TimeOnly(11, 0),
            intervalMinutes: 30
        );

        var appointment = Appointment.Create(
            storeId,
            professionalId,
            serviceId,
            "Customer",
            "+5511999999999",
            new DateTimeOffset(2026, 5, 21, 9, 30, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3))
        );

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment> { appointment });

        var slots = await _service.GetAvailableSlotsAsync(storeId, serviceId, date, professionalId);

        Assert.NotNull(slots);
        Assert.Equal(4, slots.Count);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 9, 0, 0, TimeSpan.FromHours(-3)), slots[0]);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 10, 0, 0, TimeSpan.FromHours(-3)), slots[1]);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 11, 0, 0, TimeSpan.FromHours(-3)), slots[2]);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 11, 30, 0, TimeSpan.FromHours(-3)), slots[3]);

        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _appointmentRepositoryMock.Verify(r => r.ListByProfessionalAndDateAsync(professionalId, date, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenSomeSlotsAreInThePast_ShouldFilterThemOut()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var date = new DateTimeOffset(2026, 5, 21, 0, 0, 0, TimeSpan.FromHours(-3));

        var store = Store.Create(Guid.NewGuid(), "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe");
        var profIdField = typeof(Professional).GetProperty("Id");
        profIdField?.SetValue(professional, professionalId);

        professional.AddWeeklyHours(
            dayOfWeek: 4,
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(12, 0),
            intervalMinutes: 30
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(new DateTimeOffset(2026, 5, 21, 10, 15, 0, TimeSpan.FromHours(-3)));

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var slots = await _service.GetAvailableSlotsAsync(storeId, serviceId, date, professionalId);

        Assert.NotNull(slots);
        Assert.Equal(3, slots.Count);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 10, 30, 0, TimeSpan.FromHours(-3)), slots[0]);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 11, 0, 0, TimeSpan.FromHours(-3)), slots[1]);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 11, 30, 0, TimeSpan.FromHours(-3)), slots[2]);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenMinimumLeadTimeConfigured_ShouldFilterTooCloseSlots()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var date = new DateTimeOffset(2026, 5, 21, 0, 0, 0, TimeSpan.FromHours(-3));

        var store = Store.Create(Guid.NewGuid(), "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe");
        var profIdField = typeof(Professional).GetProperty("Id");
        profIdField?.SetValue(professional, professionalId);

        professional.AddWeeklyHours(
            dayOfWeek: 4,
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(12, 0),
            intervalMinutes: 30
        );

        _timeProviderMock
            .Setup(t => t.UtcNow)
            .Returns(new DateTimeOffset(2026, 5, 21, 10, 15, 0, TimeSpan.FromHours(-3)));

        _appConfigurationMock
            .Setup(c => c.MinimumSlotLeadTimeMinutes)
            .Returns(30);

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var slots = await _service.GetAvailableSlotsAsync(storeId, serviceId, date, professionalId);

        Assert.NotNull(slots);
        Assert.Equal(2, slots.Count);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 11, 0, 0, TimeSpan.FromHours(-3)), slots[0]);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 11, 30, 0, TimeSpan.FromHours(-3)), slots[1]);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WhenTimeBlockExists_ShouldFilterOverlappingSlots()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var date = new DateTimeOffset(2026, 5, 21, 0, 0, 0, TimeSpan.FromHours(-3));

        var store = Store.Create(Guid.NewGuid(), "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);
        service.AssignProfessional(professionalId);

        var professional = Professional.Create(storeId, "Jane Doe");
        var profIdField = typeof(Professional).GetProperty("Id");
        profIdField?.SetValue(professional, professionalId);

        professional.AddWeeklyHours(
            dayOfWeek: 4,
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(12, 0),
            intervalMinutes: 30
        );

        var timeBlock = TimeBlock.Create(
            storeId,
            professionalId,
            new DateTimeOffset(2026, 5, 21, 9, 30, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 5, 21, 10, 30, 0, TimeSpan.FromHours(-3))
        );

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>())).ReturnsAsync(store);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _professionalRepositoryMock.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>())).ReturnsAsync(professional);
        _appointmentRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());
        _timeBlockRepositoryMock
            .Setup(r => r.ListByProfessionalAndDateAsync(professionalId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimeBlock> { timeBlock });

        var slots = await _service.GetAvailableSlotsAsync(storeId, serviceId, date, professionalId);

        Assert.NotNull(slots);
        Assert.Equal(4, slots.Count);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 9, 0, 0, TimeSpan.FromHours(-3)), slots[0]);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 10, 30, 0, TimeSpan.FromHours(-3)), slots[1]);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 11, 0, 0, TimeSpan.FromHours(-3)), slots[2]);
        Assert.Equal(new DateTimeOffset(2026, 5, 21, 11, 30, 0, TimeSpan.FromHours(-3)), slots[3]);
    }
}
