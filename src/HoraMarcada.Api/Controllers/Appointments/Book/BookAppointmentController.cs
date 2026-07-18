using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Appointments.Book;

[ApiController]
[Route("api/appointments")]
public class BookAppointmentController(
    BookAppointmentUseCase useCase,
    IValidator<BookAppointmentInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Books a new appointment (Public Flow).",
        Description = "Allows public scheduling of appointments. Prevents double bookings.",
        Tags = new[] { "Appointments" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the successfully booked appointment.", typeof(AppointmentDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store, professional, or service is not found.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "If there is a scheduling conflict (slot collision).", typeof(ProblemDetails))]
    public async Task<IActionResult> BookAppointment(
        [FromBody] BookAppointmentInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var appointment = await useCase.ExecuteAsync(input, cancellationToken);
        return Ok(appointment);
    }
}
