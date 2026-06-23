using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Appointments.CancelByToken;

[ApiController]
[Route("api/appointments/cancel-by-token")]
public class CancelAppointmentByTokenController(
    CancelAppointmentByTokenUseCase useCase,
    IValidator<CancelAppointmentByTokenInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cancels an appointment using a unique single-use cancellation token (Public Flow - WhatsApp Link).",
        Description = "Cancels the appointment if the secure JWT token is valid and has not been used yet.",
        Tags = new[] { "Appointments" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "The appointment was successfully canceled via token.", typeof(CancelAppointmentByTokenResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or the token is invalid, expired, or already used.", typeof(IDictionary<string, string[]>))]
    public async Task<IActionResult> CancelAppointmentByToken(
        [FromBody] CancelAppointmentByTokenInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var storeSlug = await useCase.ExecuteAsync(input.CancelToken, cancellationToken);
        return Ok(new CancelAppointmentByTokenResponse(storeSlug));
    }
}
