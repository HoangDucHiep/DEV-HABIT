using DevHabit.Api.Entities;
using DevHabit.Api.Services.Sorting;
using DevHabit.Api.Utils;

namespace DevHabit.Api.DTOs.Habits;


public static class HabitMappings
{

    public static readonly SortMappingDefinition<HabitDto, Habit> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(HabitDto.Name), nameof(Habit.Name)),
            new SortMapping(nameof(HabitDto.Description), nameof(Habit.Description)),
            new SortMapping(nameof(HabitDto.Type), nameof(Habit.Type)),
            new SortMapping(
                $"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.Type)}",
                $"{nameof(Habit.Frequency)}.{nameof(Frequency.Type)}"),
            new SortMapping(
                $"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.TimesPerPeriod)}",
                $"{nameof(Habit.Frequency)}.{nameof(Frequency.TimesPerPeriod)}"),
            new SortMapping(
                $"{nameof(HabitDto.Target)}.{nameof(TargetDto.Value)}",
                $"{nameof(Habit.Target)}.{nameof(Target.Value)}"),
            new SortMapping(
                $"{nameof(HabitDto.Target)}.{nameof(TargetDto.Unit)}",
                $"{nameof(Habit.Target)}.{nameof(Target.Unit)}"),
            new SortMapping(nameof(HabitDto.Status), nameof(Habit.Status)),
            new SortMapping(nameof(HabitDto.EndDate), nameof(Habit.EndDate)),
            new SortMapping(nameof(HabitDto.CreatedAtUtc), nameof(Habit.CreatedAtUtc)),
            new SortMapping(nameof(HabitDto.UpdatedAtUtc), nameof(Habit.UpdatedAtUtc)),
            new SortMapping(nameof(HabitDto.LastCompletedAtUtc), nameof(Habit.LastCompletedAtUtc))
        ]
    };


    public static Habit ToEntity(this CreateHabitDto dto, string userId)
    {
        Habit habit = new()
        {
            Id = IdGenerator.GenerateId(IdPrefix.HABIT),
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Frequency = new Frequency
            {
                Type = dto.Frequency.Type,
                TimesPerPeriod = dto.Frequency.TimesPerPeriod
            },
            Target = new Target
            {
                Value = dto.Target.Value,
                Unit = dto.Target.Unit
            },
            Status = HabitStatus.Ongoing,
            IsArchived = false,
            EndDate = dto.EndDate,
            Milestone = dto.Milestone is not null ? new Milestone
            {
                Target = dto.Milestone.Target,
                Current = dto.Milestone.Current
            } : null,
            CreatedAtUtc = DateTime.UtcNow,
            UserId = userId
        };

        return habit;
    }

    public static HabitDto ToDto(this Habit habit)
    {
        return new HabitDto()
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,
            Frequency = new FrequencyDto
            {
                Type = habit.Frequency.Type,
                TimesPerPeriod = habit.Frequency.TimesPerPeriod
            },
            Target = new TargetDto
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit
            },
            Status = habit.Status,
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,
            Milestone = habit.Milestone != null
                ? new MilestoneDto
                {
                    Target = habit.Milestone.Target,
                    Current = habit.Milestone.Current
                }
                : null,
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc,
            LastCompletedAtUtc = habit.LastCompletedAtUtc
        };
    }

    public static void UpdateFromDto(this Habit habit, UpdateHabitDto dto)
    {
        // update basic properties
        habit.Name = dto.Name;
        habit.Description = dto.Description;
        habit.Type = dto.Type;
        habit.EndDate = dto.EnDate;

        // update frequency
        habit.Frequency = new Frequency
        {
            Type = dto.Frequency.Type,
            TimesPerPeriod = dto.Frequency.TimesPerPeriod
        };

        // update target
        habit.Target = new Target
        {
            Value = dto.Target.Value,
            Unit = dto.Target.Unit
        };

        // update milestone if provided
        if (dto.Milestone is not null)
        {
            habit.Milestone ??= new Milestone(); // ensure Milestone is initialized
            habit.Milestone.Target = dto.Milestone.Target;
        }

        habit.UpdatedAtUtc = DateTime.UtcNow;
    }

    public static void PartialUpdateFromDto(this Habit habit, HabitDto habitDto)
    {
        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;
    }
}


