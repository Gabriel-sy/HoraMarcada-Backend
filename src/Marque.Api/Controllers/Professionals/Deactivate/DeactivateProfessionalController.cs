using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Marque.Application.UseCases.Professionals;
using Swashbuckle.AspNetCore.Annotations;

namespace Marque.Api.Controllers.Professionals.Deactivate;

[ApiController]
[Route("api/stores/{storeId}/professionals/{professionalId}/deactivate")]
public class DeactivateProfessionalController(
    DeactivateProfessionalUseCase useCase,
    IValidator<DeactivateProfessionalRequest> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Deactivates a professional in a store, preventing them from receiving new appointments.",
        Description = "Marks a professional as inactive. Only the entrepreneur owner is authorized.",
        Tags = new[] { "Professionals" }
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The professional was successfully deactivated.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to manage professionals in this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store, professional, or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> DeactivateProfessional(
        [FromRoute] Guid storeId,
        [FromRoute] Guid professionalId,
        [FromBody] DeactivateProfessionalRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        await useCase.ExecuteAsync(request.UserId, storeId, professionalId, cancellationToken);
        return NoContent();
    }
}

public record DeactivateProfessionalRequest(Guid UserId);
