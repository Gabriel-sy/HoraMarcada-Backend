using Moq;
using Xunit;
using Marque.Application.UseCases.Services;
using Marque.Application.Repositories;
using Marque.Application.Exceptions;
using Marque.Domain.Entities;

namespace Marque.UnitTests.UseCases.Services;

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
}
