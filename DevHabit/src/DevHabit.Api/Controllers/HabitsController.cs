using System.Linq.Dynamic.Core;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Commom;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services.Sorting;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<PaginationResult<HabitDto>>> GetHabits(
        // Searching
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider
    )
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid");
        }

        query.Search = query.Search?.Trim().ToLower();

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        IQueryable<HabitDto> habitQuery = dbContext.Habits
            .Where(h => query.Search == null ||
                        h.Name.ToLower().Contains(query.Search) ||
                        h.Description != null && h.Description.ToLower().Contains(query.Search))
            .Where(h => query.Type == null || h.Type == query.Type)
            .Where(h => query.Status == null || h.Status == query.Status)
            .ApplySort(query.Sort, sortMappings)
            .Select(HabitQueries.ToHabitDto());


        int totalCount = await habitQuery.CountAsync();

        List<HabitDto> habits = await habitQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var paginationResult = new PaginationResult<HabitDto>
        {
            Items = habits,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabit([FromRoute] string id)
    {
        HabitWithTagsDto habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ToHabitWithTagsDto())
            .FirstOrDefaultAsync();

        if (habit == null)
        {
            return NotFound(new
            {
                message = $"Habit with ID '{id}' not found."
            });
        }

        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto  createHabitDto, IValidator<CreateHabitDto> validator)
    {

        await validator.ValidateAndThrowAsync(createHabitDto);

        Habit habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        HabitDto habitDto = habit.ToDto();

        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id}, habitDto); 
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit([FromRoute] string id, UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await dbContext
            .Habits
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }


    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, [FromBody] JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound(new { message = $"Habit with ID '{id}' not found." });
        }

        HabitDto habitDto = habit.ToDto();

        patchDocument.ApplyTo(habitDto, ModelState);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.PartialUpdateFromDto(habitDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound(new { message = $"Habit with ID '{id}' not found." });
        }

        dbContext.Remove(habit);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
