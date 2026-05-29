using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.TimeBlocks;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.TimeBlocks;

public class ListTimeBlocksUseCaseTests
{
    private readonly Mock<ITimeBlockRepository> _timeBlockRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly ListTimeBlocksUseCase _useCase;

    public ListTimeBlocksUseCaseTests()
    {
        _timeBlockRepositoryMock = new Mock<ITimeBlockRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new ListTimeBlocksUseCase(
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
        var start = DateTimeOffset.UtcNow;
        var end = DateTimeOffset.UtcNow.AddDays(7);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId, start, end));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _timeBlockRepositoryMock.Verify(r => r.ListByProfessionalAndRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotExist_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = DateTimeOffset.UtcNow.AddDays(7);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId, start, end));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.ListByProfessionalAndRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalBelongsToOtherStore_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = DateTimeOffset.UtcNow.AddDays(7);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(otherStoreId, "Jane Doe");

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId, start, end));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.ListByProfessionalAndRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldReturnTimeBlocks()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = DateTimeOffset.UtcNow.AddDays(7);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        var timeBlocks = new List<TimeBlock>
        {
            TimeBlock.Create(storeId, professionalId, start.AddHours(1), start.AddHours(2))
        };

        _timeBlockRepositoryMock
            .Setup(r => r.ListByProfessionalAndRangeAsync(professionalId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeBlocks);

        var result = await _useCase.ExecuteAsync(userId, storeId, professionalId, start, end);

        Assert.NotNull(result);
        var singleBlock = Assert.Single(result);
        Assert.Equal(timeBlocks[0].Id, singleBlock.Id);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.ListByProfessionalAndRangeAsync(professionalId, start, end, It.IsAny<CancellationToken>()), Times.Once);
    }
}
