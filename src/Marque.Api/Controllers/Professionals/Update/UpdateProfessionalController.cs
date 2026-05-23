using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Marque.Application.UseCases.Professionals;
using Marque.Application.Models;
using Marque.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Marque.Api.Controllers.Professionals.Update;

[ApiController]
[Route("api/stores/{storeId}/professionals/{professionalId}")]
public class UpdateProfessionalController(
    UpdateProfessionalUseCase useCase,
    IValidator<UpdateProfessionalInput> validator) : ControllerBase
{
    [HttpPut]
    [SwaggerOperation(
        Summary = "Updates an existing professional's details and availability.",
        Description = "Modifies professional info and working hours. Entrepreneur authorization required.",
        Tags = new[] { "Professionals" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the updated professional details.", typeof(Professional))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to update professionals in this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store, professional, or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateProfessional(
        [FromRoute] Guid storeId,
        [FromRoute] Guid professionalId,
        [FromBody] UpdateProfessionalInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var professional = await useCase.ExecuteAsync(storeId, professionalId, input, cancellationToken);
        return Ok(professional);
    }
}
