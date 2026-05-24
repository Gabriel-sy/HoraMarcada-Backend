# Testing Guidelines

This document details how to write, structure, mock, and run tests in the Marque backend repository.

---

## 1. Test Project Structure

All unit tests are located inside the [tests/Marque.UnitTests](../tests/Marque.UnitTests) project.

The test directory layout mirrors the design of the `Marque.Application` use cases:
- `tests/Marque.UnitTests/UseCases/Stores/`
- `tests/Marque.UnitTests/UseCases/Professionals/`
- `tests/Marque.UnitTests/UseCases/Services/`
- `tests/Marque.UnitTests/UseCases/Availability/`
- `tests/Marque.UnitTests/UseCases/Appointments/`

---

## 2. Test Architecture & Libraries

- **Framework**: xUnit is used for test execution.
- **Mocking**: Moq is used to define mock behaviors for repository interfaces and external services.
- **Assertions**: Standard xUnit assertion classes (`Assert`) are used.

---

## 3. The AAA (Arrange-Act-Assert) Pattern

All unit tests must follow the AAA pattern:

### Arrange
Set up mock repositories, construct the input DTOs, and define mock return behaviors.
```csharp
var repositoryMock = new Mock<IStoreRepository>();
var input = new CreateStoreInput("Test Store", "test-store", "+5511999999999");
var userId = Guid.NewGuid();

repositoryMock
    .Setup(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((Store?)null);
```

### Act
Execute the actual method under test.
```csharp
var useCase = new CreateStoreUseCase(repositoryMock.Object);
var result = await useCase.ExecuteAsync(userId, input);
```

### Assert
Confirm the results meet expectations and verify mock interactions.
```csharp
Assert.NotNull(result);
Assert.Equal("Test Store", result.Name);

repositoryMock.Verify(
    r => r.AddAsync(It.IsAny<Store>(), It.IsAny<CancellationToken>()),
    Times.Once
);
```

---

## 4. Mocking Guidelines

- **Always verify calls**: Use `.Verify(...)` to ensure that write operations (like `AddAsync`, `UpdateAsync`) are called exactly when expected, or not called in error states (e.g., throwing exceptions).
- **Match specific parameters**: Prefer matching parameters precisely (e.g., `It.Is<Slug>(s => s.Value == input.Slug)`) rather than generic matchers (`It.IsAny<Slug>()`) to ensure that correct data is being passed through the application boundary.
- **CancellationToken**: Always pass `It.IsAny<CancellationToken>()` in the `.Setup` call to prevent cancellation tokens from breaking mock matches.

---

## 5. Command Line Reference

Execute these commands from the root directory:

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test tests/Marque.UnitTests
```

### Filter by Class
```bash
dotnet test --filter "CreateStoreUseCaseTests"
```

### Filter by Test Name
```bash
dotnet test --filter "ExecuteAsync_WhenSlugAlreadyExists_ShouldThrowConflictException"
```
