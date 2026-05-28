using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Services.Delete;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/services/{serviceId}")]
public class DeleteServiceController(DeleteServiceUseCase useCase) : ControllerBase
{
    [HttpDelete]
    [SwaggerOperation(
        Summary = "Deletes an existing service from a store.",
        Description = "Permanently deletes a service and its professional associations. Entrepreneur authorization required.",
        Tags = new[] { "Services" }
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Service deleted successfully.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to delete services in this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or service is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteService(
        [FromRoute] Guid storeId,
        [FromRoute] Guid serviceId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await useCase.ExecuteAsync(userId, storeId, serviceId, cancellationToken);
        return NoContent();
    }
}
