using FluentValidation;

namespace DevHabit.Api.DTOs.Tags;

public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tag name is required.")
            .Must(x => x.Length >= 3 && x.Length <= 50)
            .WithMessage("Tag name must have between 3 and 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(50);
    }   
}
