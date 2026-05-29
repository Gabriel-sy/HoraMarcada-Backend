using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.TimeBlocks;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.TimeBlocks;

public class DeleteTimeBlockUseCaseTests
{
    private readonly Mock<ITimeBlockRepository> _timeBlockRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly DeleteTimeBlockUseCase _useCase;

    public DeleteTimeBlockUseCaseTests()
    {
        _timeBlockRepositoryMock = new Mock<ITimeBlockRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new DeleteTimeBlockUseCase(
            _timeBlockRepositoryMock.Object,
            _ownershipPolicyMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var blockId = Guid.NewGuid();

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, blockId));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _timeBlockRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<TimeBlock>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBlockDoesNotExist_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var blockId = Guid.NewGuid();

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _timeBlockRepositoryMock
            .Setup(r => r.GetByIdAsync(blockId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeBlock?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, blockId));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.GetByIdAsync(blockId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<TimeBlock>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBlockBelongsToOtherStore_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var blockId = Guid.NewGuid();

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var timeBlock = TimeBlock.Create(otherStoreId, Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        _timeBlockRepositoryMock
            .Setup(r => r.GetByIdAsync(blockId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeBlock);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, blockId));

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.GetByIdAsync(blockId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<TimeBlock>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldDeleteTimeBlock()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var blockId = Guid.NewGuid();

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var timeBlock = TimeBlock.Create(storeId, Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        _timeBlockRepositoryMock
            .Setup(r => r.GetByIdAsync(blockId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeBlock);

        await _useCase.ExecuteAsync(userId, storeId, blockId);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.GetByIdAsync(blockId, It.IsAny<CancellationToken>()), Times.Once);
        _timeBlockRepositoryMock.Verify(r => r.DeleteAsync(timeBlock, It.IsAny<CancellationToken>()), Times.Once);
    }
}
