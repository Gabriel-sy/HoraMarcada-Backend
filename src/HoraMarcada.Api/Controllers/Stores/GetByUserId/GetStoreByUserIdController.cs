using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Stores;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Stores.GetByUserId;

[Authorize]
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
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the authenticated user ID does not match the requested user ID.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If no store is found for the given User ID.", typeof(ProblemDetails))]
    public async Task<IActionResult> GetStoreByUserId(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.GetUserId();
        if (authenticatedUserId != userId)
        {
            return Forbid();
        }

        var store = await useCase.ExecuteAsync(userId, cancellationToken);
        return Ok(store);
    }
}
