using Moq;
using Xunit;
using Marque.Application.UseCases.Professionals;
using Marque.Application.Repositories;
using Marque.Application.Policies;
using Marque.Application.Models;
using Marque.Application.Exceptions;
using Marque.Domain.Entities;

namespace Marque.UnitTests.UseCases.Professionals;

public class UpdateProfessionalUseCaseTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly UpdateProfessionalUseCase _useCase;

    public UpdateProfessionalUseCaseTests()
    {
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new UpdateProfessionalUseCase(
            _professionalRepositoryMock.Object,
            _ownershipPolicyMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        // Arrange
        var input = new UpdateProfessionalInput(
            UserId: Guid.NewGuid(),
            StoreId: Guid.NewGuid(),
            ProfessionalId: Guid.NewGuid(),
            Name: "Jane Doe"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var input = new UpdateProfessionalInput(
            UserId: Guid.NewGuid(),
            StoreId: Guid.NewGuid(),
            ProfessionalId: Guid.NewGuid(),
            Name: "Jane Doe"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(input.ProfessionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(input.ProfessionalId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotBelongToStore_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new UpdateProfessionalInput(
            UserId: userId,
            StoreId: storeId,
            ProfessionalId: professionalId,
            Name: "Jane Doe"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(otherStoreId, "Jane Doe");
        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldUpdateProfessionalAndReplaceWeeklyHours()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var weeklyHours = new List<WeeklyHoursInput>
        {
            new WeeklyHoursInput(
                DayOfWeek: 2,
                StartTime: new TimeOnly(10, 0),
                EndTime: new TimeOnly(17, 0),
                LunchStart: new TimeOnly(13, 0),
                LunchEnd: new TimeOnly(14, 0),
                IntervalMinutes: 45
            )
        };

        var input = new UpdateProfessionalInput(
            UserId: userId,
            StoreId: storeId,
            ProfessionalId: professionalId,
            Name: "Jane Smith",
            PhotoUrl: "http://photo.com/janesmith",
            WhatsApp: "+5511888888888",
            WeeklyHours: weeklyHours
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(1, new TimeOnly(9, 0), new TimeOnly(18, 0)); // Old hours

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.PhotoUrl, result.PhotoUrl);
        Assert.Equal(input.WhatsApp, result.WhatsApp?.Value);
        
        var singleSchedule = Assert.Single(result.WeeklySchedules);
        Assert.Equal(2, singleSchedule.DayOfWeek);
        Assert.Equal(new TimeOnly(10, 0), singleSchedule.StartTime);
        Assert.Equal(new TimeOnly(17, 0), singleSchedule.EndTime);
        Assert.Equal(new TimeOnly(13, 0), singleSchedule.LunchStart);
        Assert.Equal(new TimeOnly(14, 0), singleSchedule.LunchEnd);
        Assert.Equal(45, singleSchedule.IntervalMinutes);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
    }
}
