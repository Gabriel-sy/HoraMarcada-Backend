using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.Services;

public class ListProfessionalsForServiceUseCaseTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly ListProfessionalsForServiceUseCase _useCase;

    public ListProfessionalsForServiceUseCaseTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _useCase = new ListProfessionalsForServiceUseCase(
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(serviceId));
        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceExists_ShouldReturnActiveAssignedProfessionals()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var activeProfId = Guid.NewGuid();
        var inactiveProfId = Guid.NewGuid();
        var nonExistentProfId = Guid.NewGuid();

        var service = Service.Create(storeId, "Service Name", 30);
        var serviceIdField = typeof(Service).GetProperty("Id");
        serviceIdField?.SetValue(service, serviceId);

        service.AssignProfessional(activeProfId);
        service.AssignProfessional(inactiveProfId);
        service.AssignProfessional(nonExistentProfId);

        var activeProf = Professional.Create(storeId, "Active Prof", active: true);
        var profIdField = typeof(Professional).GetProperty("Id");
        profIdField?.SetValue(activeProf, activeProfId);

        var inactiveProf = Professional.Create(storeId, "Inactive Prof", active: false);
        profIdField?.SetValue(inactiveProf, inactiveProfId);
        // Force deactivation just in case
        inactiveProf.Deactivate();

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(activeProfId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeProf);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(inactiveProfId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveProf);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentProfId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act
        var result = await _useCase.ExecuteAsync(serviceId);

        // Assert
        Assert.NotNull(result);
        var singleProf = Assert.Single(result);
        Assert.Equal(activeProfId, singleProf.Id);
        Assert.Equal("Active Prof", singleProf.Name);

        _serviceRepositoryMock.Verify(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(activeProfId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(inactiveProfId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(nonExistentProfId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceExists_ShouldReturnProfessionalsOrderedByName()
    {
        var storeId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var profAId = Guid.NewGuid();
        var profBId = Guid.NewGuid();
        var profCId = Guid.NewGuid();

        var service = Service.Create(storeId, "Service Name", 30);
        typeof(Service).GetProperty("Id")?.SetValue(service, serviceId);

        service.AssignProfessional(profCId);
        service.AssignProfessional(profAId);
        service.AssignProfessional(profBId);

        var profA = Professional.Create(storeId, "Ana", active: true);
        typeof(Professional).GetProperty("Id")?.SetValue(profA, profAId);

        var profB = Professional.Create(storeId, "Bruno", active: true);
        typeof(Professional).GetProperty("Id")?.SetValue(profB, profBId);

        var profC = Professional.Create(storeId, "Carlos", active: true);
        typeof(Professional).GetProperty("Id")?.SetValue(profC, profCId);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(profAId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profA);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(profBId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profB);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(profCId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profC);

        var result = await _useCase.ExecuteAsync(serviceId);

        Assert.Equal(new[] { "Ana", "Bruno", "Carlos" }, result.Select(p => p.Name));
    }
}
