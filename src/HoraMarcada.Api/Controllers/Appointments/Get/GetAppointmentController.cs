using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Appointments.Get;

[Authorize]
[ApiController]
[Route("api/appointments/{appointmentId}")]
public class GetAppointmentController(GetAppointmentByIdUseCase useCase) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Gets a single appointment by ID (Entrepreneur Panel).",
        Description = "Retrieves the full appointment details including customer data. Only the store owner is authorized.",
        Tags = new[] { "Appointments" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the appointment.", typeof(Appointment))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the appointment is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> GetAppointment(
        [FromRoute] Guid appointmentId,
        [FromQuery] Guid storeId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        try
        {
            var appointment = await useCase.ExecuteAsync(userId, appointmentId, storeId, cancellationToken);
            return Ok(appointment);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
