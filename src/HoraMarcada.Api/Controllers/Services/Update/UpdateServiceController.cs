using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Application.Models;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Services.Update;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/services/{serviceId}")]
public class UpdateServiceController(
    UpdateServiceUseCase useCase,
    IValidator<UpdateServiceInput> validator) : ControllerBase
{
    [HttpPut]
    [SwaggerOperation(
        Summary = "Updates an existing service in a store.",
        Description = "Updates the service details and its associated professionals. Entrepreneur authorization required.",
        Tags = new[] { "Services" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the updated service details.", typeof(Service))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to update services in this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store, service, or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateService(
        [FromRoute] Guid storeId,
        [FromRoute] Guid serviceId,
        [FromBody] UpdateServiceInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var userId = User.GetUserId();
        var service = await useCase.ExecuteAsync(userId, storeId, serviceId, input, cancellationToken);
        return Ok(service);
    }
}
