## 1. Domain Entities and Value Objects

To maintain strict invariants, follow these rules when adding or modifying domain objects:

### Entities
- **Encapsulated State**: All properties must have `private set` or `init` accessors. State changes must happen via explicit domain methods.
- **Private Constructor**: Provide a private parameterless constructor. EF Core requires this constructor to instantiate entities when materializing queries.
- **Factory Methods**: Create new instances using a public static `Create` method that validates initial invariants before instantiating the class.

Example Entity outline:
```csharp
namespace Marque.Domain.Entities;

public sealed class SampleEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private SampleEntity()
    {
    }

    public static SampleEntity Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.");
        }

        return new SampleEntity
        {
            Id = Guid.NewGuid(),
            Name = name.Trim()
        };
    }
}
```

### Value Objects
- Define value objects as C# `readonly record` or `record` types to get built-in value-based equality.
- Enforce validation rules inside the constructor or factory method.

Example Value Object:
```csharp
namespace Marque.Domain.ValueObjects;

public sealed record SampleValue
{
    public string Value { get; }

    private SampleValue(string value) => Value = value;

    public static SampleValue Create(string value)
    {
        if (value.Length < 3)
        {
            throw new ArgumentException("Value must be at least 3 characters.");
        }

        return new SampleValue(value);
    }
}
```

---

## 2. Use Case Patterns

- Each use case must have a single public method (e.g., `ExecuteAsync`) that orchestrates a specific business process.
- Use constructor dependency injection to pass repository ports and core service contracts.
- Do not reference database concepts (such as SQL, EF Core contexts, transactions) inside the Use Cases. Depend only on repository ports (e.g., `IStoreRepository`).

Example Use Case:
```csharp
namespace Marque.Application.UseCases.Stores;

public class CreateStoreUseCase
{
    private readonly IStoreRepository _storeRepository;

    public CreateStoreUseCase(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<Store> ExecuteAsync(Guid userId, CreateStoreInput input, CancellationToken cancellationToken = default)
    {
        var store = Store.Create(userId, input.Name, input.Slug, input.WhatsApp);
        await _storeRepository.AddAsync(store, cancellationToken);
        return store;
    }
}
```

---

## 3. Controller-Per-Action (CPA) Pattern

The API project organizes endpoints by action, not by entity controllers with multiple routes.

### Folder Structure
Group each route under a folder named after its action. The folder should contain:
1. The controller class containing exactly one HTTP route method.
2. The FluentValidation request validator class.

Example directory path:
`src/Marque.Api/Controllers/Stores/Create/`
- `CreateStoreController.cs`
- `CreateStoreRequestValidator.cs`

### Validator Integration
Inject `IValidator<T>` into the controller constructor and call `ValidateAsync` manually before calling the use case.

Example Controller & Validator:
```csharp
namespace Marque.Api.Controllers.Stores.Create;

[ApiController]
[Route("api/stores")]
public class CreateStoreController : ControllerBase
{
    private readonly CreateStoreUseCase _useCase;
    private readonly IValidator<CreateStoreInput> _validator;

    public CreateStoreController(CreateStoreUseCase useCase, IValidator<CreateStoreInput> validator)
    {
        _useCase = useCase;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateStore([FromBody] CreateStoreInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var store = await _useCase.ExecuteAsync(Guid.NewGuid(), input, cancellationToken);
        return Ok(store);
    }
}
```

---

## 4. Error Handling and Exception Flow

The API project maps business/validation exceptions to standard RFC 7807 `ProblemDetails` structures.

- **Use Cases** throw domain exceptions or application exceptions (e.g., `NotFoundException`, `ConflictException`, `DomainValidationException`).
- **Global Exception Filter**: Registered in `Program.cs`, the `GlobalExceptionHandler` interceptor catches these exceptions and maps them to HTTP status codes.

Exception mapping rules:
- `NotFoundException` -> HTTP 404 (Not Found)
- `ConflictException` -> HTTP 409 (Conflict)
- `DomainValidationException` -> HTTP 400 (Bad Request)
- Any other unhandled exception -> HTTP 500 (Internal Server Error)
