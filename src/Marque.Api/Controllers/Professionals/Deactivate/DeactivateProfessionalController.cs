using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Marque.Application.UseCases.Professionals;
using Marque.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace Marque.Api.Controllers.Professionals.Deactivate;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/professionals/{professionalId}/deactivate")]
public class DeactivateProfessionalController(
    DeactivateProfessionalUseCase useCase) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Deactivates a professional in a store, preventing them from receiving new appointments.",
        Description = "Marks a professional as inactive. Only the entrepreneur owner is authorized.",
        Tags = new[] { "Professionals" }
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The professional was successfully deactivated.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to manage professionals in this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store, professional, or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> DeactivateProfessional(
        [FromRoute] Guid storeId,
        [FromRoute] Guid professionalId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await useCase.ExecuteAsync(userId, storeId, professionalId, cancellationToken);
        return NoContent();
    }
}
