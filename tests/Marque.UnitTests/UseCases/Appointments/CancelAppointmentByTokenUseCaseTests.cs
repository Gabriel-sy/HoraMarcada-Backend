using Moq;
using Xunit;
using Marque.Application.UseCases.Appointments;
using Marque.Application.Repositories;
using Marque.Application.Exceptions;
using Marque.Domain.Entities;
using Marque.Domain.Exceptions;
using Marque.Domain.Enums;

namespace Marque.UnitTests.UseCases.Appointments;

public class CancelAppointmentByTokenUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly CancelAppointmentByTokenUseCase _useCase;

    public CancelAppointmentByTokenUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _useCase = new CancelAppointmentByTokenUseCase(_appointmentRepositoryMock.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WhenCancelTokenIsEmpty_ShouldThrowDomainValidationException(string? token)
    {
        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(token!));
#pragma warning restore CS8604

        _appointmentRepositoryMock.Verify(r => r.GetByCancelTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAppointmentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var token = "token123";
        _appointmentRepositoryMock
            .Setup(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(token));

        _appointmentRepositoryMock.Verify(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelTokenAlreadyUsed_ShouldThrowConflictException()
    {
        // Arrange
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

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(token));

        _appointmentRepositoryMock.Verify(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidToken_ShouldCancelAndMarkTokenUsed()
    {
        // Arrange
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

        _appointmentRepositoryMock
            .Setup(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        await _useCase.ExecuteAsync(token);

        // Assert
        Assert.Equal(AppointmentStatus.Canceled, appointment.Status);
        Assert.True(appointment.CancelTokenUsed);
        
        _appointmentRepositoryMock.Verify(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Appointment>(a => 
            a.Id == appointment.Id &&
            a.Status == AppointmentStatus.Canceled &&
            a.CancelTokenUsed == true
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
