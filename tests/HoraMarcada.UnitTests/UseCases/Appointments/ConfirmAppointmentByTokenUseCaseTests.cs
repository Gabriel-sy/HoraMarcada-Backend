using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;
using HoraMarcada.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HoraMarcada.UnitTests.UseCases.Appointments;

public class ConfirmAppointmentByTokenUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly ConfirmAppointmentByTokenUseCase _useCase;

    public ConfirmAppointmentByTokenUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _useCase = new ConfirmAppointmentByTokenUseCase(_appointmentRepositoryMock.Object);
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
    public async Task ExecuteAsync_WhenValidToken_ShouldConfirmPresence()
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

        _appointmentRepositoryMock
            .Setup(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await _useCase.ExecuteAsync(token);

        Assert.Equal(AppointmentStatus.ConfirmedByUser, appointment.Status);

        _appointmentRepositoryMock.Verify(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Appointment>(a =>
            a.Id == appointment.Id &&
            a.Status == AppointmentStatus.ConfirmedByUser
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAppointmentCanceled_ShouldThrowDomainValidationException()
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
        appointment.Cancel();

        _appointmentRepositoryMock
            .Setup(r => r.GetByCancelTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(token));

        _appointmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
