using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Stores;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.Stores;

public class GetStoreByUserIdUseCaseTests
{
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly GetStoreByUserIdUseCase _useCase;

    public GetStoreByUserIdUseCaseTests()
    {
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _useCase = new GetStoreByUserIdUseCase(_storeRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _storeRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId));
        _storeRepositoryMock.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreExists_ShouldReturnStore()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var store = Store.Create(
            userId,
            "My Store",
            "my-store",
            "+5511999999999",
            "store@example.com"
        );

        _storeRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(store.Id, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("My Store", result.Name);
        _storeRepositoryMock.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
