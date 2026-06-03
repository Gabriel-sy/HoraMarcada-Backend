using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HoraMarcada.Application.UseCases.Customers;
using HoraMarcada.Application.Models;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Customers.List;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/customers")]
public class ListCustomersController(ListCustomersUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lists customers for a store (Entrepreneur Panel).",
        Description = "Retrieves a paginated list of customers with optional search filtering by name or WhatsApp.",
        Tags = new[] { "Customers" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the paginated customer list.", typeof(PagedResult<CustomerListDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user does not own this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> ListCustomers(
        [FromRoute] Guid storeId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await useCase.ExecuteAsync(userId, storeId, search, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
