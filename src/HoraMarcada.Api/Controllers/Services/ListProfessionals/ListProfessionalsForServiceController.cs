using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Services.ListProfessionals;

[ApiController]
[Route("api/stores/{storeId}/services/{serviceId}/professionals")]
public class ListProfessionalsForServiceController(ListProfessionalsForServiceUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lists all professionals linked to a specific service in a store.",
        Description = "Retrieves professionals who are qualified to perform the specified service within the store.",
        Tags = new[] { "Services" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the list of professionals who perform the service.", typeof(IReadOnlyList<Professional>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or service is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> ListProfessionalsForService(
        [FromRoute] Guid storeId,
        [FromRoute] Guid serviceId,
        CancellationToken cancellationToken)
    {
        var professionals = await useCase.ExecuteAsync(serviceId, cancellationToken);
        return Ok(professionals);
    }
}
