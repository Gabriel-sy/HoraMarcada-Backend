using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Common;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Enums;

namespace HoraMarcada.UnitTests.UseCases.Appointments;

public class CancelAppointmentUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CancelAppointmentUseCase _useCase;

    public CancelAppointmentUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _notificationServiceMock = new Mock<INotificationService>();

        _useCase = new CancelAppointmentUseCase(
            _appointmentRepositoryMock.Object,
            _storeRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _ownershipPolicyMock.Object,
            _notificationServiceMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var input = new CancelAppointmentInput(storeId);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, appointmentId, input));
        
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAppointmentDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var input = new CancelAppointmentInput(storeId);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, appointmentId, input));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAppointmentDoesNotBelongToStore_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var input = new CancelAppointmentInput(storeId);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var appointment = Appointment.Create(
            otherStoreId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2)
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, appointmentId, input));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldCancelAppointment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var input = new CancelAppointmentInput(storeId);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            "customer@example.com",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2)
        );

        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Service", 30);
        var professional = Professional.Create(storeId, "Professional", email: "professional@example.com");

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
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

        // Act
        await _useCase.ExecuteAsync(userId, appointmentId, input);

        // Assert
        Assert.Equal(AppointmentStatus.Canceled, appointment.Status);
        
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Appointment>(a => 
            a.Id == appointment.Id &&
            a.Status == AppointmentStatus.Canceled
        ), It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendCancellationNoticeToCustomerAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendCancellationNotificationToStoreAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendCancellationNotificationToProfessionalAsync(
            appointment, store, service, professional, It.IsAny<CancellationToken>()), Times.Once);
    }
}
