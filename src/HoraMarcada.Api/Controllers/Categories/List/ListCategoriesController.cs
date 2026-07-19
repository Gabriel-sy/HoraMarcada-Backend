using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Categories;
using HoraMarcada.Application.Models;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Categories.List;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/categories")]
public class ListCategoriesController(ListCategoriesUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lists categories for a store.",
        Description = "Retrieves all categories for the specified store. Entrepreneur authorization required.",
        Tags = new[] { "Categories" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the list of categories.", typeof(IReadOnlyList<CategoryDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to list categories for this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> ListCategories(
        [FromRoute] Guid storeId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var categories = await useCase.ExecuteAsync(userId, storeId, cancellationToken);
        return Ok(categories);
    }
}
