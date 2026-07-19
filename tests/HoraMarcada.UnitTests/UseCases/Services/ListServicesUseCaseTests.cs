using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.Services;

public class ListServicesUseCaseTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly ListServicesUseCase _useCase;

    public ListServicesUseCaseTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _useCase = new ListServicesUseCase(
            _serviceRepositoryMock.Object,
            _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActiveOnlyIsFalse_ShouldReturnAllServices()
    {
        var storeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var service1 = Service.Create(storeId, categoryId, "Service 1", 30, active: true);
        var service2 = Service.Create(storeId, categoryId, "Service 2", 60, active: false);
        var serviceList = new List<Service> { service1, service2 }.AsReadOnly();

        var category = Category.Create(storeId, "Geral");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category, categoryId);

        _serviceRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceList);

        _categoryRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { category }.AsReadOnly());

        var result = await _useCase.ExecuteAsync(storeId, activeOnly: false);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == service1.Id);
        Assert.Contains(result, r => r.Id == service2.Id);

        _serviceRepositoryMock.Verify(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActiveOnlyIsTrue_ShouldReturnOnlyActiveServices()
    {
        var storeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var service1 = Service.Create(storeId, categoryId, "Service 1", 30, active: true);
        var service2 = Service.Create(storeId, categoryId, "Service 2", 60, active: false);
        var serviceList = new List<Service> { service1, service2 }.AsReadOnly();

        var category = Category.Create(storeId, "Geral");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category, categoryId);

        _serviceRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceList);

        _categoryRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { category }.AsReadOnly());

        var result = await _useCase.ExecuteAsync(storeId, activeOnly: true);

        Assert.NotNull(result);
        var singleService = Assert.Single(result);
        Assert.Equal(service1.Id, singleService.Id);

        _serviceRepositoryMock.Verify(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnCategoryIdAndCategoryNameForEachService()
    {
        var storeId = Guid.NewGuid();
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var service1 = Service.Create(storeId, categoryId1, "Service A", 30);
        var service2 = Service.Create(storeId, categoryId2, "Service B", 45);
        var serviceList = new List<Service> { service1, service2 }.AsReadOnly();

        var category1 = Category.Create(storeId, "Cortes");
        var category2 = Category.Create(storeId, "Barba");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category1, categoryId1);
        categoryIdField?.SetValue(category2, categoryId2);

        _serviceRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceList);

        _categoryRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { category1, category2 }.AsReadOnly());

        var result = await _useCase.ExecuteAsync(storeId, activeOnly: false);

        Assert.Equal(2, result.Count);

        var dto1 = result.Single(r => r.Id == service1.Id);
        Assert.Equal(categoryId1, dto1.CategoryId);
        Assert.Equal("Cortes", dto1.CategoryName);

        var dto2 = result.Single(r => r.Id == service2.Id);
        Assert.Equal(categoryId2, dto2.CategoryId);
        Assert.Equal("Barba", dto2.CategoryName);
    }
}
