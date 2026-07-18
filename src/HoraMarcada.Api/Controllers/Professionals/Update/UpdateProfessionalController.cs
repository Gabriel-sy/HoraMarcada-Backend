using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Professionals;
using HoraMarcada.Application.Models;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Professionals.Update;

[Authorize]
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
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the updated professional details.", typeof(ProfessionalDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
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

        var userId = User.GetUserId();
        var professional = await useCase.ExecuteAsync(userId, storeId, professionalId, input, cancellationToken);
        return Ok(professional);
    }
}
