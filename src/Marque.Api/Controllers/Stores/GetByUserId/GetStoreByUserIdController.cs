using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Marque.Application.UseCases.Stores;
using Marque.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Marque.Api.Controllers.Stores.GetByUserId;

[ApiController]
[Route("api/stores/user/{userId}")]
public class GetStoreByUserIdController(GetStoreByUserIdUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves the store associated with a specific User ID.",
        Description = "Retrieves the store owned by the entrepreneur matching the provided user ID.",
        Tags = new[] { "Stores" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the store details.", typeof(Store))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If no store is found for the given User ID.", typeof(ProblemDetails))]
    public async Task<IActionResult> GetStoreByUserId(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var store = await useCase.ExecuteAsync(userId, cancellationToken);
        return Ok(store);
    }
}
