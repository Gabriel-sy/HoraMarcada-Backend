using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Stores;
using HoraMarcada.Application.Models;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Stores.GetStatistics;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/statistics")]
public class GetStoreStatisticsController(
    GetStoreStatisticsUseCase useCase,
    IValidator<GetStoreStatisticsInput> validator) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves store statistics (Entrepreneur Dashboard).",
        Description = "Retrieves dashboard KPIs and appointment analytics for a specific store.",
        Tags = new[] { "Stores" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the store statistics.", typeof(StoreStatisticsResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to view statistics for this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> GetStoreStatistics(
        [FromRoute] Guid storeId,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken cancellationToken)
    {
        var resolvedStart = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var resolvedEnd = endDate ?? DateTimeOffset.UtcNow;

        var input = new GetStoreStatisticsInput(storeId, resolvedStart, resolvedEnd);
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var userId = User.GetUserId();
        var statistics = await useCase.ExecuteAsync(userId, input, cancellationToken);

        return Ok(statistics);
    }
}
