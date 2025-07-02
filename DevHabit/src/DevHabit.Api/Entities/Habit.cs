namespace DevHabit.Api.Entities;

public sealed class Habit
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HabitType Type { get; set; }
    public Frequency Frequency { get; set; }
    public Target Target { get; set; }
    public HabitStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public DateOnly? EndDate { get; set; } // Optional end date for the habit
    public Milestone? Milestone { get; set; } // Optional milestone for the habit
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc{ get; set; }
    public DateTime? LastCompletedAtUtc { get; set; }

    public List<HabitTag> HabitTags { get; set; }
    public List<Tag> Tags { get; set; }

    public string UserId { get; set; }
}

public enum HabitType
{
    None = 0,
    Binary = 1, // Yes/No
    Measurable = 2, // Numeric value
}

public enum HabitStatus
{
    None = 0,
    Ongoing = 1, // Habit is currently being practiced
    Completed = 2, // Habit has been completed
}

public sealed class Frequency
{
    public FrequencyType Type { get; set; }
    public int TimesPerPeriod { get; set; } // How many times per period (e.g., 3 times per week)
}

public enum FrequencyType
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4,
}

public sealed class Target
{
    public int Value { get; set; } // Target value (e.g., 10 minutes, 5 items)
    public string Unit { get; set; }
}

public sealed class Milestone
{
    public int Target { get; set; } // Target value for the milestone
    public int Current { get; set; } // Current progress towards the milestone
}
