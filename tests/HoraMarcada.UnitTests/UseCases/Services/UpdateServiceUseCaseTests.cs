using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.UnitTests.UseCases.Services;

public class UpdateServiceUseCaseTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly UpdateServiceUseCase _useCase;

    public UpdateServiceUseCaseTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new UpdateServiceUseCase(
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _ownershipPolicyMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var input = new UpdateServiceInput(
            Name: "Updated Haircut",
            DurationMinutes: 45,
            CategoryId: categoryId
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, serviceId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceDoesNotExist_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var input = new UpdateServiceInput(
            Name: "Updated Haircut",
            DurationMinutes: 45,
            CategoryId: categoryId
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, serviceId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceDoesNotBelongToStore_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var input = new UpdateServiceInput(
            Name: "Updated Haircut",
            DurationMinutes: 45,
            CategoryId: categoryId
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = Service.Create(otherStoreId, Guid.NewGuid(), "Old Haircut", 30);
        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, serviceId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryIdIsEmpty_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var input = new UpdateServiceInput(
            Name: "Updated Haircut",
            DurationMinutes: 45,
            CategoryId: Guid.Empty
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = Service.Create(storeId, Guid.NewGuid(), "Old Haircut", 30);
        var idField = typeof(Service).GetProperty("Id");
        idField?.SetValue(service, serviceId);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, serviceId, input));
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryDoesNotExist_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var input = new UpdateServiceInput(
            Name: "Updated Haircut",
            DurationMinutes: 45,
            CategoryId: categoryId
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = Service.Create(storeId, Guid.NewGuid(), "Old Haircut", 30);
        var idField = typeof(Service).GetProperty("Id");
        idField?.SetValue(service, serviceId);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, serviceId, input));
        _serviceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryBelongsToAnotherStore_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var input = new UpdateServiceInput(
            Name: "Updated Haircut",
            DurationMinutes: 45,
            CategoryId: categoryId
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = Service.Create(storeId, Guid.NewGuid(), "Old Haircut", 30);
        var idField = typeof(Service).GetProperty("Id");
        idField?.SetValue(service, serviceId);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var category = Category.Create(otherStoreId, "Cortes");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category, categoryId);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, serviceId, input));
        _serviceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotBelongToStore_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var invalidProfId = Guid.NewGuid();

        var input = new UpdateServiceInput(
            Name: "Updated Haircut",
            DurationMinutes: 45,
            CategoryId: categoryId,
            ProfessionalIds: [invalidProfId]
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = Service.Create(storeId, Guid.NewGuid(), "Old Haircut", 30);
        var idField = typeof(Service).GetProperty("Id");
        idField?.SetValue(service, serviceId);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var category = Category.Create(storeId, "Cortes");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category, categoryId);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var otherStoreProfessional = Professional.Create(Guid.NewGuid(), "Jane Doe");
        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(invalidProfId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherStoreProfessional);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, serviceId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(invalidProfId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldUpdateServiceReassignProfessionalsAndChangeCategory()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var oldCategoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();

        var input = new UpdateServiceInput(
            Name: "Updated Haircut",
            DurationMinutes: 45,
            CategoryId: newCategoryId,
            Price: 60.0m,
            Active: true,
            ProfessionalIds: [professionalId]
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = Service.Create(storeId, oldCategoryId, "Old Haircut", 30);
        var idField = typeof(Service).GetProperty("Id");
        idField?.SetValue(service, serviceId);
        service.AssignProfessional(Guid.NewGuid());

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var category = Category.Create(storeId, "Barba");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category, newCategoryId);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var professional = Professional.Create(storeId, "Jane Doe");
        var profIdField = typeof(Professional).GetProperty("Id");
        profIdField?.SetValue(professional, professionalId);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        var result = await _useCase.ExecuteAsync(userId, storeId, serviceId, input);

        Assert.NotNull(result);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.DurationMinutes, result.DurationMinutes);
        Assert.Equal(input.Price, result.Price);
        Assert.Equal(input.Active, result.Active);
        Assert.Equal(newCategoryId, result.CategoryId);
        Assert.Equal("Barba", result.CategoryName);
        Assert.Single(result.ProfessionalIds);
        Assert.Contains(professionalId, result.ProfessionalIds);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.UpdateAsync(service, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(newCategoryId, service.CategoryId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryChanges_ShouldNotAffectProfessionalLinks()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var oldCategoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();

        var input = new UpdateServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            CategoryId: newCategoryId,
            ProfessionalIds: [professionalId]
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = Service.Create(storeId, oldCategoryId, "Haircut", 30);
        var idField = typeof(Service).GetProperty("Id");
        idField?.SetValue(service, serviceId);
        service.AssignProfessional(professionalId);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var category = Category.Create(storeId, "Barba");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category, newCategoryId);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var professional = Professional.Create(storeId, "Jane Doe");
        var profIdField = typeof(Professional).GetProperty("Id");
        profIdField?.SetValue(professional, professionalId);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        var result = await _useCase.ExecuteAsync(userId, storeId, serviceId, input);

        Assert.Equal(newCategoryId, result.CategoryId);
        Assert.Contains(professionalId, result.ProfessionalIds);
        Assert.Contains(professionalId, service.ProfessionalIds);
    }
}
