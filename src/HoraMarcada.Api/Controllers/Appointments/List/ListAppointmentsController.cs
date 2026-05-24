using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.Models;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Appointments.List;

[Authorize]
[ApiController]
[Route("api/appointments")]
public class ListAppointmentsController(
    ListAppointmentsUseCase useCase,
    IValidator<ListAppointmentsInput> validator) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lists all appointments for a store (Entrepreneur Panel).",
        Description = "Retrieves a list of appointments matching the requested date range and filters. Authorized entrepreneur only.",
        Tags = new[] { "Appointments" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the list of appointments.", typeof(IReadOnlyList<Appointment>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to view appointments for this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> ListAppointments(
        [FromQuery] ListAppointmentsInput request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var userId = User.GetUserId();
        var appointments = await useCase.ExecuteAsync(
            userId,
            request,
            cancellationToken
        );

        return Ok(appointments);
    }
}
