using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Models;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.UnitTests.UseCases.Services;

public class AddServiceUseCaseTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly AddServiceUseCase _useCase;

    public AddServiceUseCaseTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new AddServiceUseCase(
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
        var categoryId = Guid.NewGuid();
        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            CategoryId: categoryId
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryIdIsEmpty_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            CategoryId: Guid.Empty
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryDoesNotExist_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            CategoryId: categoryId
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryBelongsToAnotherStore_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            CategoryId: categoryId
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var category = Category.Create(otherStoreId, "Cortes");
        var idField = typeof(Category).GetProperty("Id");
        idField?.SetValue(category, categoryId);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotBelongToStore_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            CategoryId: categoryId,
            ProfessionalIds: [professionalId]
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var category = Category.Create(storeId, "Cortes");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category, categoryId);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var professional = Professional.Create(otherStoreId, "John Doe");
        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotExist_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            CategoryId: categoryId,
            ProfessionalIds: [professionalId]
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var category = Category.Create(storeId, "Cortes");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category, categoryId);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldCreateAndAssignProfessionals()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            CategoryId: categoryId,
            Price: 50.0m,
            ProfessionalIds: [professionalId]
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var category = Category.Create(storeId, "Cortes");
        var categoryIdField = typeof(Category).GetProperty("Id");
        categoryIdField?.SetValue(category, categoryId);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var professional = Professional.Create(storeId, "John Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        var result = await _useCase.ExecuteAsync(userId, storeId, input);

        Assert.NotNull(result);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.DurationMinutes, result.DurationMinutes);
        Assert.Equal(input.Price, result.Price);
        Assert.Contains(professionalId, result.ProfessionalIds);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.Is<Service>(s =>
            s.Id == result.Id &&
            s.StoreId == storeId &&
            s.CategoryId == categoryId &&
            s.Name == input.Name &&
            s.DurationMinutes == input.DurationMinutes &&
            s.Price.Amount == input.Price &&
            s.ProfessionalIds.Contains(professionalId)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
