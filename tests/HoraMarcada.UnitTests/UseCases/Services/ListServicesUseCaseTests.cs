using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.Services;

public class ListServicesUseCaseTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly ListServicesUseCase _useCase;

    public ListServicesUseCaseTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _useCase = new ListServicesUseCase(_serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActiveOnlyIsFalse_ShouldReturnAllServices()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var service1 = Service.Create(storeId, "Service 1", 30, active: true);
        var service2 = Service.Create(storeId, "Service 2", 60, active: false);
        var serviceList = new List<Service> { service1, service2 }.AsReadOnly();

        _serviceRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceList);

        // Act
        var result = await _useCase.ExecuteAsync(storeId, activeOnly: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == service1.Id);
        Assert.Contains(result, r => r.Id == service2.Id);

        _serviceRepositoryMock.Verify(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActiveOnlyIsTrue_ShouldReturnOnlyActiveServices()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var service1 = Service.Create(storeId, "Service 1", 30, active: true);
        var service2 = Service.Create(storeId, "Service 2", 60, active: false);
        var serviceList = new List<Service> { service1, service2 }.AsReadOnly();

        _serviceRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceList);

        // Act
        var result = await _useCase.ExecuteAsync(storeId, activeOnly: true);

        // Assert
        Assert.NotNull(result);
        var singleService = Assert.Single(result);
        Assert.Equal(service1.Id, singleService.Id);

        _serviceRepositoryMock.Verify(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
