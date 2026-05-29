using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.TimeBlocks;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.TimeBlocks.List;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/professionals/{professionalId}/blocks")]
public class ListTimeBlocksController(ListTimeBlocksUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lists time blocks for a professional in a date range.",
        Description = "Retrieves all administrative time blocks for a specific professional in the store. Entrepreneur authorization required.",
        Tags = new[] { "TimeBlocks" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the list of time blocks.", typeof(IReadOnlyList<TimeBlock>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user does not own this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or professional is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> ListTimeBlocks(
        [FromRoute] Guid storeId,
        [FromRoute] Guid professionalId,
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var blocks = await useCase.ExecuteAsync(userId, storeId, professionalId, start, end, cancellationToken);
        return Ok(blocks);
    }
}
