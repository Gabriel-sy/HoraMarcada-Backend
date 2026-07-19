using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Common.Interfaces;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;
using HoraMarcada.Domain.Enums;

namespace HoraMarcada.UnitTests.UseCases.Appointments;

public class CancelAppointmentByTokenUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IAppointmentNotificationService> _appointmentNotificationServiceMock;
    private readonly CancelAppointmentByTokenUseCase _useCase;

    public CancelAppointmentByTokenUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _appointmentNotificationServiceMock = new Mock<IAppointmentNotificationService>();

        _useCase = new CancelAppointmentByTokenUseCase(
            _appointmentRepositoryMock.Object,
            _storeRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _appointmentNotificationServiceMock.Object
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WhenCancelTokenIsEmpty_ShouldThrowDomainValidationException(string? token)
    {
#pragma warning disable CS8604
        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(token!));
#pragma warning restore CS8604

        _appointmentRepositoryMock.Verify(r => r.GetByCancelTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAppointmentNotFound_ShouldThrowNotFoundException()
    {
        var token = "token123";
        _appointmentRepositoryMock
            .Setup(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(token));

        _appointmentRepositoryMock.Verify(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelTokenAlreadyUsed_ShouldThrowConflictException()
    {
        var token = "token123";
        var appointment = Appointment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            cancelToken: token
        );
        appointment.MarkCancelTokenUsed();

        _appointmentRepositoryMock
            .Setup(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(token));

        _appointmentRepositoryMock.Verify(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidToken_ShouldCancelAndMarkTokenUsed()
    {
        var token = "token123";
        var storeId = Guid.NewGuid();
        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            cancelToken: token
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Service", 30);
        var professional = Professional.Create(storeId, "Professional");

        _appointmentRepositoryMock
            .Setup(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.ProfessionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        var result = await _useCase.ExecuteAsync(token);

        Assert.Equal("store", result);
        Assert.Equal(AppointmentStatus.Canceled, appointment.Status);
        Assert.True(appointment.CancelTokenUsed);
        
        _appointmentRepositoryMock.Verify(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Appointment>(a => 
            a.Id == appointment.Id &&
            a.Status == AppointmentStatus.Canceled &&
            a.CancelTokenUsed == true
        ), It.IsAny<CancellationToken>()), Times.Once);

        _appointmentNotificationServiceMock.Verify(n => n.DispatchCancellationNotificationsAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);
    }
}
