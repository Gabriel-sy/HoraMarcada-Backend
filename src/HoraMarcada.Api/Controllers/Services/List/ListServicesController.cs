using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Services.List;

[ApiController]
[Route("api/stores/{storeId}/services")]
public class ListServicesController(ListServicesUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lists all services associated with a store.",
        Description = "Retrieves services registered in the store, optionally filtering by active status.",
        Tags = new[] { "Services" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the list of services.", typeof(IReadOnlyList<ServiceDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> ListServices(
        [FromRoute] Guid storeId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var services = await useCase.ExecuteAsync(storeId, activeOnly, cancellationToken);
        return Ok(services);
    }
}
