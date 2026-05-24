using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Availability;
using HoraMarcada.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Availability.GetSlots;

[ApiController]
[Route("api/stores/{storeId}/availability")]
public class GetAvailableSlotsController(
    GetAvailableSlotsUseCase useCase,
    IValidator<GetAvailableSlotsInput> validator) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves available time slots (start times) for appointments.",
        Description = "Calculates available slots by checking store hours, lunch times, and existing bookings. Public flow.",
        Tags = new[] { "Availability" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the list of available start times.", typeof(IReadOnlyList<DateTimeOffset>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or service is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> GetAvailableSlots(
        [FromRoute] Guid storeId,
        [FromQuery] GetAvailableSlotsInput request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var slots = await useCase.ExecuteAsync(
            storeId,
            request.ServiceId,
            request.Date,
            request.ProfessionalId,
            cancellationToken
        );

        return Ok(slots);
    }
}
