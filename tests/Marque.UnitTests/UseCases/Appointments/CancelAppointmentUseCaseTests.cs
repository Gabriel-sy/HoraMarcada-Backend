using Moq;
using Xunit;
using Marque.Application.UseCases.Appointments;
using Marque.Application.Repositories;
using Marque.Application.Policies;
using Marque.Application.Exceptions;
using Marque.Domain.Entities;
using Marque.Domain.Enums;

namespace Marque.UnitTests.UseCases.Appointments;

public class CancelAppointmentUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly CancelAppointmentUseCase _useCase;

    public CancelAppointmentUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new CancelAppointmentUseCase(
            _appointmentRepositoryMock.Object,
            _ownershipPolicyMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, appointmentId));
        
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

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, appointmentId));

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

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var appointment = Appointment.Create(
            otherStoreId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2)
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, appointmentId));

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

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var appointment = Appointment.Create(
            storeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Customer",
            "+5511999999999",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2)
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        await _useCase.ExecuteAsync(userId, storeId, appointmentId);

        // Assert
        Assert.Equal(AppointmentStatus.Canceled, appointment.Status);
        
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Appointment>(a => 
            a.Id == appointment.Id &&
            a.Status == AppointmentStatus.Canceled
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
