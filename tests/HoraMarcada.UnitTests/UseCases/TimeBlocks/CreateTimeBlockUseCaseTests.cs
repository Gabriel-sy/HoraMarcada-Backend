using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.TimeBlocks;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.TimeBlocks;

public class CreateTimeBlockUseCaseTests
{
    private readonly Mock<ITimeBlockRepository> _timeBlockRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly CreateTimeBlockUseCase _useCase;

    public CreateTimeBlockUseCaseTests()
    {
        _timeBlockRepositoryMock = new Mock<ITimeBlockRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new CreateTimeBlockUseCase(
            _timeBlockRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _ownershipPolicyMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var input = new CreateTimeBlockInput(
            StartTime: DateTimeOffset.UtcNow,
            EndTime: DateTimeOffset.UtcNow.AddHours(1)
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId, input));
        
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TimeBlock>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotExist_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var input = new CreateTimeBlockInput(
            StartTime: DateTimeOffset.UtcNow,
            EndTime: DateTimeOffset.UtcNow.AddHours(1)
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId, input));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TimeBlock>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalBelongsToOtherStore_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var input = new CreateTimeBlockInput(
            StartTime: DateTimeOffset.UtcNow,
            EndTime: DateTimeOffset.UtcNow.AddHours(1)
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(otherStoreId, "Jane Doe");

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId, input));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TimeBlock>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldCreateTimeBlock()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var input = new CreateTimeBlockInput(
            StartTime: DateTimeOffset.UtcNow.AddHours(1),
            EndTime: DateTimeOffset.UtcNow.AddHours(2)
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        var result = await _useCase.ExecuteAsync(userId, storeId, professionalId, input);

        Assert.NotNull(result);
        Assert.Equal(storeId, result.StoreId);
        Assert.Equal(professionalId, result.ProfessionalId);
        Assert.Equal(input.StartTime.ToUniversalTime(), result.StartTime);
        Assert.Equal(input.EndTime.ToUniversalTime(), result.EndTime);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.AddAsync(It.Is<TimeBlock>(t =>
            t.Id == result.Id &&
            t.StoreId == storeId &&
            t.ProfessionalId == professionalId &&
            t.StartTime == input.StartTime.ToUniversalTime() &&
            t.EndTime == input.EndTime.ToUniversalTime()
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
