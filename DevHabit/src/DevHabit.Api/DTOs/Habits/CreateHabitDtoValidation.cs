using DevHabit.Api.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Connections.Features;

namespace DevHabit.Api.DTOs.Habits;

public sealed class CreateHabitDtoValidation : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] _allowedUnits =
    [
        "minutes",
        "hours",
        "steps",
        "km",
        "cal",
        "pages",
        "books",
        "tasks",
        "sessions"
    ];

    private static readonly string[] _allowedUnitsForBinaryHabits =
    [
        "sessions",
        "tasks"
    ];

    public CreateHabitDtoValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("Habit name is required and must be between 3 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null)
            .WithMessage("Description must be less than 500 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Habit type is required and must be a valid value.");

        RuleFor(x => x.Frequency.Type)
            .IsInEnum()
            .WithMessage("Frequency type is required and must be a valid value.");

        RuleFor(x => x.Frequency.TimesPerPeriod)
            .GreaterThan(0)
            .WithMessage("Frequency times per period must be greater than 0.");

        RuleFor(x => x.Target.Value)
            .GreaterThan(0)
            .WithMessage("Target value must be greater than 0.");

        RuleFor(x => x.Target.Unit)
            .NotEmpty()
            .Must(x => _allowedUnits.Contains(x.ToLowerInvariant()))
            .WithMessage($"Target unit is required and must be one of the following: {string.Join(", ", _allowedUnits)}.");

        RuleFor(x => x.EndDate)
            .Must(date => date is null || date.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date must be in the future.");

        When(x => x.Milestone is not null, () =>
        {
            RuleFor(x => x.Milestone!.Target)
                .GreaterThan(0)
                .WithMessage("Milestone target value must be greater than 0.");
        });

        RuleFor(x => x.Target.Unit)
            .Must((dto, unit) => IsTargetUnitCompatibleWithType(dto.Type, unit))
            .WithMessage("Target unit is not ompatible with the habit type.");
    }

    private static bool IsTargetUnitCompatibleWithType(HabitType type, string unit)
    {
        var normalizedUnit = unit.ToLowerInvariant();

        return type switch
        {
            HabitType.Binary => _allowedUnitsForBinaryHabits.Contains(normalizedUnit),
            HabitType.Messurable => _allowedUnits.Contains(normalizedUnit),
            _ => false
        };
    }
}
