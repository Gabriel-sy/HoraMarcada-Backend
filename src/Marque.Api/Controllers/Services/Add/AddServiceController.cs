using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Marque.Application.UseCases.Services;
using Marque.Application.Models;
using Marque.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Marque.Api.Controllers.Services.Add;

[ApiController]
[Route("api/stores/{storeId}/services")]
public class AddServiceController(
    AddServiceUseCase useCase,
    IValidator<AddServiceInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Adds a new service to a store.",
        Description = "Creates a new service linked to the store and associates it with professionals. Entrepreneur authorization required.",
        Tags = new[] { "Services" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the newly added service details.", typeof(Service))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to add services to this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> AddService(
        [FromRoute] Guid storeId,
        [FromBody] AddServiceInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var service = await useCase.ExecuteAsync(storeId, input, cancellationToken);
        return Ok(service);
    }
}
