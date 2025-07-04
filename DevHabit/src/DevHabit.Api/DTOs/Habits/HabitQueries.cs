﻿
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;
public static class HabitQueries
{
    public static System.Linq.Expressions.Expression<Func<Habit, HabitDto>> ToHabitDto()
    {
        return h => new HabitDto
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            Type = h.Type,
            Frequency = new FrequencyDto
            {
                Type = h.Frequency.Type,
                TimesPerPeriod = h.Frequency.TimesPerPeriod
            },
            Target = new TargetDto
            {
                Value = h.Target.Value,
                Unit = h.Target.Unit
            },
            Status = h.Status,
            IsArchived = h.IsArchived,
            EndDate = h.EndDate,
            Milestone = h.Milestone != null ? new MilestoneDto
            {
                Target = h.Milestone.Target,
                Current = h.Milestone.Current
            } : null,
            CreatedAtUtc = h.CreatedAtUtc,
            UpdatedAtUtc = h.UpdatedAtUtc,
            LastCompletedAtUtc = h.LastCompletedAtUtc
        };
    }

    public static System.Linq.Expressions.Expression<Func<Habit, HabitWithTagsDto>> ToHabitWithTagsDto()
    {
        return h => new HabitWithTagsDto
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            Type = h.Type,
            Frequency = new FrequencyDto
            {
                Type = h.Frequency.Type,
                TimesPerPeriod = h.Frequency.TimesPerPeriod
            },
            Target = new TargetDto
            {
                Value = h.Target.Value,
                Unit = h.Target.Unit
            },
            Status = h.Status,
            IsArchived = h.IsArchived,
            EndDate = h.EndDate,
            Milestone = h.Milestone != null ? new MilestoneDto
            {
                Target = h.Milestone.Target,
                Current = h.Milestone.Current
            } : null,
            CreatedAtUtc = h.CreatedAtUtc,
            UpdatedAtUtc = h.UpdatedAtUtc,
            LastCompletedAtUtc = h.LastCompletedAtUtc,
            Tags = h.Tags.Select(t => t.Name).ToArray()
        };
    }


    public static System.Linq.Expressions.Expression<Func<Habit, HabitWithTagsDtoV2>> ToHabitWithTagsDtoV2()
    {
        return h => new HabitWithTagsDtoV2
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            Type = h.Type,
            Frequency = new FrequencyDto
            {
                Type = h.Frequency.Type,
                TimesPerPeriod = h.Frequency.TimesPerPeriod
            },
            Target = new TargetDto
            {
                Value = h.Target.Value,
                Unit = h.Target.Unit
            },
            Status = h.Status,
            IsArchived = h.IsArchived,
            EndDate = h.EndDate,
            Milestone = h.Milestone != null ? new MilestoneDto
            {
                Target = h.Milestone.Target,
                Current = h.Milestone.Current
            } : null,
            CreatedAt = h.CreatedAtUtc,
            UpdatedAt = h.UpdatedAtUtc,
            LastCompletedAt = h.LastCompletedAtUtc,
            Tags = h.Tags.Select(t => t.Name).ToArray()
        };
    }
}
