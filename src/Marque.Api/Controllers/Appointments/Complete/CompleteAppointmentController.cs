using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Marque.Application.UseCases.Appointments;
using Marque.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Marque.Api.Controllers.Appointments.Complete;

[ApiController]
[Route("api/appointments/{appointmentId}/complete")]
public class CompleteAppointmentController(
    CompleteAppointmentUseCase useCase,
    IValidator<CompleteAppointmentInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Marks an existing appointment as completed (Entrepreneur Panel).",
        Description = "Completes the active appointment. Only the entrepreneur owner is authorized.",
        Tags = new[] { "Appointments" }
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The appointment was successfully marked as completed.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to complete appointments in this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store, appointment, or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> CompleteAppointment(
        [FromRoute] Guid appointmentId,
        [FromBody] CompleteAppointmentInput input,
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
