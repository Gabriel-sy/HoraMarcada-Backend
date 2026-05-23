using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Marque.Application.UseCases.Stores;
using Marque.Application.Models;
using Marque.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Marque.Api.Controllers.Stores.Create;

[ApiController]
[Route("api/stores")]
public class CreateStoreController(
    CreateStoreUseCase useCase,
    IValidator<CreateStoreInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Creates a new store.",
        Description = "Creates a new store linked to the entrepreneur user. The store slug must be unique.",
        Tags = new[] { "Stores" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the newly created store details.", typeof(Store))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "If the store slug is already taken.", typeof(ProblemDetails))]
    public async Task<IActionResult> CreateStore(
        [FromBody] CreateStoreInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var store = await useCase.ExecuteAsync(input, cancellationToken);
        return Ok(store);
    }
}
