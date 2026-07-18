using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Customers;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HoraMarcada.UnitTests.UseCases.Customers;

public class CustomerUseCasesTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;

    public CustomerUseCasesTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
    }

    [Fact]
    public async Task ListCustomers_ShouldCallRepositoryAndReturnPagedResult()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var search = "test";
        var page = 1;
        var pageSize = 10;

        var items = new List<CustomerListDto>
        {
            new(Guid.NewGuid(), storeId, "Name", "+5511999999999", 2, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        };

        _customerRepositoryMock
            .Setup(r => r.ListWithStatsByStoreAsync(storeId, search, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 1));

        var useCase = new ListCustomersUseCase(_customerRepositoryMock.Object, _ownershipPolicyMock.Object);

        var result = await useCase.ExecuteAsync(userId, storeId, search, page, pageSize);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomer_WhenExists_ShouldReturnDetail()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var detail = new CustomerDetailDto(customerId, storeId, "Name", "+5511999999999", DateTimeOffset.UtcNow, new List<CustomerAppointmentHistoryDto>());

        _customerRepositoryMock
            .Setup(r => r.GetDetailByIdAsync(storeId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        var useCase = new GetCustomerUseCase(_customerRepositoryMock.Object, _ownershipPolicyMock.Object);

        var result = await useCase.ExecuteAsync(userId, storeId, customerId);

        Assert.NotNull(result);
        Assert.Equal(customerId, result.Id);
    }

    [Fact]
    public async Task GetCustomer_WhenDoesNotExist_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetDetailByIdAsync(storeId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDetailDto?)null);

        var useCase = new GetCustomerUseCase(_customerRepositoryMock.Object, _ownershipPolicyMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => useCase.ExecuteAsync(userId, storeId, customerId));
    }

    [Fact]
    public async Task UpdateCustomer_WhenValid_ShouldUpdateAndReturnCustomer()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var input = new UpdateCustomerInput("New Name", "+5511999999998");

        var customer = Customer.Create(storeId, "Old Name", "+5511999999999");
        var idField = typeof(Customer).GetProperty("Id");
        idField?.SetValue(customer, customerId);

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(r => r.GetByWhatsAppAsync(storeId, input.WhatsApp, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var useCase = new UpdateCustomerUseCase(_customerRepositoryMock.Object, _ownershipPolicyMock.Object);

        var result = await useCase.ExecuteAsync(userId, storeId, customerId, input);

        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("+5511999999998", result.WhatsApp);
        _customerRepositoryMock.Verify(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_WhenWhatsAppExistsForOtherCustomer_ShouldThrowConflictException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var input = new UpdateCustomerInput("New Name", "+5511999999998");

        var customer = Customer.Create(storeId, "Old Name", "+5511999999999");
        var idField = typeof(Customer).GetProperty("Id");
        idField?.SetValue(customer, customerId);

        var otherCustomer = Customer.Create(storeId, "Other Name", "+5511999999998");
        idField?.SetValue(otherCustomer, otherCustomerId);

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(r => r.GetByWhatsAppAsync(storeId, input.WhatsApp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherCustomer);

        var useCase = new UpdateCustomerUseCase(_customerRepositoryMock.Object, _ownershipPolicyMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() => useCase.ExecuteAsync(userId, storeId, customerId, input));
    }
}
