using DevHabit.Api.DTOs.Commom;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.DTOs.Habits;

public sealed record HabitsQueryParameters : AcceptHeaderDto
{
    [FromQuery]
    public string? Search { get; set; }
    [FromQuery(Name = "type")]
    public HabitType? Type { get; set; }
    [FromQuery(Name = "status")]
    public HabitStatus? Status { get; set; }
    [FromQuery(Name = "sort")]
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    // Data shaping
    public string? Fields { get; init; }
}
