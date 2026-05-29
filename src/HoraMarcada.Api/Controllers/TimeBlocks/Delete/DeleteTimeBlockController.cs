using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.TimeBlocks;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.TimeBlocks.Delete;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/professionals/{professionalId}/blocks/{blockId}")]
public class DeleteTimeBlockController(DeleteTimeBlockUseCase useCase) : ControllerBase
{
    [HttpDelete]
    [SwaggerOperation(
        Summary = "Deletes a time block.",
        Description = "Removes an administrative time block for a professional. Entrepreneur authorization required.",
        Tags = new[] { "TimeBlocks" }
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "If the time block was successfully deleted.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user does not own this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or time block is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteTimeBlock(
        [FromRoute] Guid storeId,
        [FromRoute] Guid blockId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await useCase.ExecuteAsync(userId, storeId, blockId, cancellationToken);
        return NoContent();
    }
}
