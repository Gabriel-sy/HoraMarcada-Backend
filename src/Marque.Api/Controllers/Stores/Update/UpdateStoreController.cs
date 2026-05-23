using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Marque.Application.UseCases.Stores;
using Marque.Application.Models;
using Marque.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Marque.Api.Controllers.Stores.Update;

[ApiController]
[Route("api/stores/{storeId}")]
public class UpdateStoreController(
    UpdateStoreUseCase useCase,
    IValidator<UpdateStoreInput> validator) : ControllerBase
{
    [HttpPut]
    [SwaggerOperation(
        Summary = "Updates an existing store.",
        Description = "Updates the store details. Only the authorized entrepreneur owner can perform this action.",
        Tags = new[] { "Stores" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the updated store details.", typeof(Store))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to update this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or user is not found.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "If the new store slug is already taken by another store.", typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateStore(
        [FromRoute] Guid storeId,
        [FromBody] UpdateStoreInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var store = await useCase.ExecuteAsync(storeId, input, cancellationToken);
        return Ok(store);
    }
}
