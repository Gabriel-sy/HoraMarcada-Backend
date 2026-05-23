using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Marque.Application.UseCases.Appointments;
using Marque.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Marque.Api.Controllers.Appointments.Cancel;

[ApiController]
[Route("api/appointments/{appointmentId}/cancel")]
public class CancelAppointmentController(
    CancelAppointmentUseCase useCase,
    IValidator<CancelAppointmentInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cancels an existing appointment (Entrepreneur Panel).",
        Description = "Cancels the confirmed appointment. Only the entrepreneur owner is authorized.",
        Tags = new[] { "Appointments" }
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The appointment was successfully canceled.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to cancel appointments in this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store, appointment, or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> CancelAppointment(
        [FromRoute] Guid appointmentId,
        [FromBody] CancelAppointmentInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        await useCase.ExecuteAsync(appointmentId, input, cancellationToken);
        return NoContent();
    }
}
