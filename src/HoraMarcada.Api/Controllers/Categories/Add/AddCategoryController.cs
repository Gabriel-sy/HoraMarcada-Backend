using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Categories;
using HoraMarcada.Application.Models;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Categories.Add;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/categories")]
public class AddCategoryController(
    CreateCategoryUseCase useCase,
    IValidator<CreateCategoryInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Creates a new category for a store.",
        Description = "Creates a store-scoped category used to group services. Entrepreneur authorization required.",
        Tags = new[] { "Categories" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the newly created category.", typeof(CategoryDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to add categories to this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or user is not found.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "If a category with the same name already exists in this store.", typeof(ProblemDetails))]
    public async Task<IActionResult> AddCategory(
        [FromRoute] Guid storeId,
        [FromBody] CreateCategoryInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var userId = User.GetUserId();
        var category = await useCase.ExecuteAsync(userId, storeId, input, cancellationToken);
        return Ok(category);
    }
}
