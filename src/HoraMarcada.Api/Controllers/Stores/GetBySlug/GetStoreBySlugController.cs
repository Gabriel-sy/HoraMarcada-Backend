using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Stores;
using HoraMarcada.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Stores.GetBySlug;

[ApiController]
[Route("api/stores/slug/{slug}")]
public class GetStoreBySlugController(GetStoreBySlugUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves the store associated with a specific Slug.",
        Description = "Retrieves store details for the public page matching the provided slug.",
        Tags = new[] { "Stores" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the store details.", typeof(StoreDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If no store is found for the given Slug.", typeof(ProblemDetails))]
    public async Task<IActionResult> GetStoreBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var store = await useCase.ExecuteAsync(slug, cancellationToken);
        return Ok(store);
    }
}
