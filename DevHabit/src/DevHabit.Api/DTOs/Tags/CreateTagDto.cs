using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace DevHabit.Api.DTOs.Tags;

public class CreateTagDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Description).MaximumLength(50);
    }
}
