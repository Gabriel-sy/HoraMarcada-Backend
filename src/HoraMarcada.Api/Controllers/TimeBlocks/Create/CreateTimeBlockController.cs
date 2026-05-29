using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.TimeBlocks;
using HoraMarcada.Application.Models;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.TimeBlocks.Create;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/professionals/{professionalId}/blocks")]
public class CreateTimeBlockController(
    CreateTimeBlockUseCase useCase,
    IValidator<CreateTimeBlockInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Creates a new administrative time block for a professional.",
        Description = "Blocks a time range to prevent customers from scheduling appointments. Entrepreneur authorization required.",
        Tags = new[] { "TimeBlocks" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the newly created time block details.", typeof(TimeBlock))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or range is invalid.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user does not own this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or professional is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> CreateTimeBlock(
        [FromRoute] Guid storeId,
        [FromRoute] Guid professionalId,
        [FromBody] CreateTimeBlockInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var userId = User.GetUserId();
        var timeBlock = await useCase.ExecuteAsync(userId, storeId, professionalId, input, cancellationToken);
        return Ok(timeBlock);
    }
}
