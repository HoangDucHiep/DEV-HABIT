using DevHabit.Api.Database;
using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using DevHabit.Api.Utils;

namespace DevHabit.Api.Extensions;

public static class SeedDataExtensions
{
    public static async Task SeedDataAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await dbContext.Habits.AnyAsync())
        {
            List<Habit> habits = GetSeedHabits();
            await dbContext.Habits.AddRangeAsync(habits);
            await dbContext.SaveChangesAsync();
            app.Logger.LogInformation("Seeded {Count} habits into the database.", habits.Count);
        }
        else
        {
            app.Logger.LogInformation("Database already contains habits, skipping seeding.");
        }
    }


    private static List<Habit> GetSeedHabits()
    {
        return new List<Habit>
        {
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Morning Exercise",
                Description = "30 minutes of morning workout to start the day energetically",
                Type = HabitType.Measurable,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new Target
                {
                    Value = 30,
                    Unit = "minutes"
                },
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = null,
                Milestone = new Milestone
                {
                    Target = 100,
                    Current = 25
                },
                CreatedAtUtc = DateTime.UtcNow.AddDays(-30),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-1),
                LastCompletedAtUtc = DateTime.UtcNow.AddDays(-1)
            },
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Read Books",
                Description = "Reading at least 20 pages per day to improve knowledge",
                Type = HabitType.Measurable,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new Target
                {
                    Value = 20,
                    Unit = "pages"
                },
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = null,
                Milestone = new Milestone
                {
                    Target = 50,
                    Current = 35
                },
                CreatedAtUtc = DateTime.UtcNow.AddDays(-20),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-2),
                LastCompletedAtUtc = DateTime.UtcNow.AddDays(-1)
            },
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Drink Water",
                Description = "Stay hydrated by drinking enough water throughout the day",
                Type = HabitType.Measurable,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 8
                },
                Target = new Target
                {
                    Value = 8,
                    Unit = "glasses"
                },
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = null,
                Milestone = new Milestone
                {
                    Target = 200,
                    Current = 150
                },
                CreatedAtUtc = DateTime.UtcNow.AddDays(-45),
                UpdatedAtUtc = DateTime.UtcNow,
                LastCompletedAtUtc = DateTime.UtcNow
            },
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Meditation",
                Description = "Daily meditation for mental wellness and stress relief",
                Type = HabitType.Binary,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new Target
                {
                    Value = 15,
                    Unit = "minutes"
                },
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = null,
                Milestone = null,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-15),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-3),
                LastCompletedAtUtc = DateTime.UtcNow.AddDays(-2)
            },
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Learn Programming",
                Description = "Weekly coding practice to improve programming skills",
                Type = HabitType.Measurable,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Weekly,
                    TimesPerPeriod = 3
                },
                Target = new Target
                {
                    Value = 2,
                    Unit = "hours"
                },
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(6)),
                Milestone = new Milestone
                {
                    Target = 20,
                    Current = 8
                },
                CreatedAtUtc = DateTime.UtcNow.AddDays(-60),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-5),
                LastCompletedAtUtc = DateTime.UtcNow.AddDays(-3)
            },
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Social Media Detox",
                Description = "Completed 30-day challenge to reduce social media usage",
                Type = HabitType.Binary,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new Target
                {
                    Value = 0,
                    Unit = "hours"
                },
                Status = HabitStatus.Completed,
                IsArchived = true,
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                Milestone = new Milestone
                {
                    Target = 30,
                    Current = 30
                },
                CreatedAtUtc = DateTime.UtcNow.AddDays(-35),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-5),
                LastCompletedAtUtc = DateTime.UtcNow.AddDays(-5)
            },
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Yoga Practice",
                Description = "Weekly yoga sessions for flexibility and mental peace",
                Type = HabitType.Binary,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Weekly,
                    TimesPerPeriod = 2
                },
                Target = new Target
                {
                    Value = 45,
                    Unit = "minutes"
                },
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = null,
                Milestone = new Milestone
                {
                    Target = 24,
                    Current = 12
                },
                CreatedAtUtc = DateTime.UtcNow.AddDays(-40),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-7),
                LastCompletedAtUtc = DateTime.UtcNow.AddDays(-4)
            },
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Write Journal",
                Description = "Daily journaling to reflect on thoughts and experiences",
                Type = HabitType.Binary,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new Target
                {
                    Value = 10,
                    Unit = "minutes"
                },
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = null,
                Milestone = new Milestone
                {
                    Target = 365,
                    Current = 180
                },
                CreatedAtUtc = DateTime.UtcNow.AddDays(-180),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-1),
                LastCompletedAtUtc = DateTime.UtcNow.AddDays(-1)
            },
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Monthly Budget Review",
                Description = "Review and update personal budget every month",
                Type = HabitType.Binary,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Monthly,
                    TimesPerPeriod = 1
                },
                Target = new Target
                {
                    Value = 60,
                    Unit = "minutes"
                },
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                Milestone = new Milestone
                {
                    Target = 12,
                    Current = 3
                },
                CreatedAtUtc = DateTime.UtcNow.AddDays(-90),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-30),
                LastCompletedAtUtc = DateTime.UtcNow.AddDays(-30)
            },
            new Habit
            {
                Id = IdGenerator.GenerateId("h_"),
                Name = "Learn New Language",
                Description = "Practice Spanish using language learning app",
                Type = HabitType.Measurable,
                Frequency = new Frequency
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new Target
                {
                    Value = 25,
                    Unit = "minutes"
                },
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(12)),
                Milestone = new Milestone
                {
                    Target = 365,
                    Current = 42
                },
                CreatedAtUtc = DateTime.UtcNow.AddDays(-42),
                UpdatedAtUtc = DateTime.UtcNow,
                LastCompletedAtUtc = DateTime.UtcNow
            }
        };
    }
}
