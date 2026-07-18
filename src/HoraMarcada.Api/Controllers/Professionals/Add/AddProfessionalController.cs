using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HoraMarcada.Application.UseCases.Professionals;
using HoraMarcada.Application.Models;
using HoraMarcada.Api.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace HoraMarcada.Api.Controllers.Professionals.Add;

[Authorize]
[ApiController]
[Route("api/stores/{storeId}/professionals")]
public class AddProfessionalController(
    AddProfessionalUseCase useCase,
    IValidator<AddProfessionalInput> validator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Adds a new professional to a store.",
        Description = "Registers a new professional and defines their initial weekly working hours. Entrepreneur authorization required.",
        Tags = new[] { "Professionals" }
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the newly added professional details.", typeof(ProfessionalDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "If validation fails or request contains invalid properties.", typeof(IDictionary<string, string[]>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "If the user is not authenticated.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "If the user is not authorized to add professionals to this store.", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "If the store or user is not found.", typeof(ProblemDetails))]
    public async Task<IActionResult> AddProfessional(
        [FromRoute] Guid storeId,
        [FromBody] AddProfessionalInput input,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var userId = User.GetUserId();
        var professional = await useCase.ExecuteAsync(userId, storeId, input, cancellationToken);
        return Ok(professional);
    }
}
