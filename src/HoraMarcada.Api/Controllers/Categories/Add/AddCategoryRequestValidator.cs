using FluentValidation;
using HoraMarcada.Application.Models;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Api.Controllers.Categories.Add;

public class AddCategoryRequestValidator : AbstractValidator<CreateCategoryInput>
{
    public AddCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(Category.MaxNameLength)
            .WithMessage($"Category name cannot exceed {Category.MaxNameLength} characters.");
    }
}
