using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HoraMarcada.Application.UseCases.Customers;
using HoraMarcada.Application.Models;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Customers.Update;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/customers/{customerId}")]
public class UpdateCustomerController(
    UpdateCustomerUseCase useCase,
    IValidator<UpdateCustomerInput> validator) : ControllerBase
{
    [HttpPut]
    [SwaggerOperation(
        Summary = "Updates customer profile details (Entrepreneur Panel).",
        Description = "Updates a customer's basic contact details.",
        Tags = new[] { "Customers" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the updated customer.", typeof(Customer))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user does not own this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or customer is not found.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "If the new WhatsApp number is already registered for another customer.", typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateCustomer(
        [FromRoute] Guid storeId,
        [FromRoute] Guid customerId,
        [FromBody] UpdateCustomerInput input,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var userId = User.GetUserId();
        var result = await useCase.ExecuteAsync(userId, storeId, customerId, input, cancellationToken);
        return Ok(result);
    }
}
