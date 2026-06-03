using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HoraMarcada.Application.UseCases.Customers;
using HoraMarcada.Application.Models;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Customers.Get;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/customers/{customerId}")]
public class GetCustomerController(GetCustomerUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves details of a customer (Entrepreneur Panel).",
        Description = "Retrieves profile and historical appointments timeline of a specific customer.",
        Tags = new[] { "Customers" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the customer details with history.", typeof(CustomerDetailDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user does not own this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or customer is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> GetCustomer(
        [FromRoute] Guid storeId,
        [FromRoute] Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await useCase.ExecuteAsync(userId, storeId, customerId, cancellationToken);
        return Ok(result);
    }
}
