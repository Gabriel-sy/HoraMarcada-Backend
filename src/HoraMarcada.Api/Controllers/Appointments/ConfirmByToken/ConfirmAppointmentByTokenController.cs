using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Appointments.ConfirmByToken;

[ApiController]
[Route("api/appointments/confirm-by-token")]
public class ConfirmAppointmentByTokenController(
    ConfirmAppointmentByTokenUseCase useCase,
    IValidator<ConfirmAppointmentByTokenInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Confirms presence for an appointment using the secure token (Public Flow).",
        Description = "Sets the appointment status to ConfirmedByUser if the secure token is valid.",
        Tags = new[] { "Appointments" }
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The appointment presence was successfully confirmed.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or the token is invalid, expired, or already used.", typeof(IDictionary<string, string[]>))]
    public async Task<IActionResult> ConfirmAppointmentByToken(
        [FromBody] ConfirmAppointmentByTokenInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        await useCase.ExecuteAsync(input.CancelToken, cancellationToken);
        return NoContent();
    }
}
