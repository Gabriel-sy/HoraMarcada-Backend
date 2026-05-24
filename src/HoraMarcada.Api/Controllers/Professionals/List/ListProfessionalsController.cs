using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Professionals;
using HoraMarcada.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Professionals.List;

[ApiController]
[Route("api/stores/{storeId}/professionals")]
public class ListProfessionalsController(ListProfessionalsUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lists all professionals associated with a store.",
        Description = "Retrieves a list of all registered professionals in the specified store.",
        Tags = new[] { "Professionals" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the list of professionals.", typeof(IReadOnlyList<Professional>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> ListProfessionals(
        [FromRoute] Guid storeId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var professionals = await useCase.ExecuteAsync(storeId, activeOnly, cancellationToken);
        return Ok(professionals);
    }
}
