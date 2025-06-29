using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Net.Mime;
using Asp.Versioning;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Commom;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
public sealed class HabitsController(ApplicationDbContext dbContext, LinkService linkService) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult> GetHabits(
        // Searching
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService
    )
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid");
        }

        if (!dataShapingService.Validate<HabitDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided fields parameter isn't valid. "
            );
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

        bool includeLinks = query.Accept == CustomMediaTypeNames.Application.HateoasJson;

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeCollectionData(
                habits, 
                query.Fields, 
                includeLinks ? h => CreateLinksForHabit(h.Id, query.Fields) : null),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
        };

        if (includeLinks)
        {
            paginationResult.Links = CreateLinksForHabits(
                query,
                paginationResult.HasNextPage,
                paginationResult.HasPreviousPage);
        }

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetHabit(
        [FromRoute] string id, 
        string? fields,
        [FromHeader(Name = "Accept")] string? accept,
        DataShapingService dataShapingService)
    {
        if (!dataShapingService.Validate<HabitWithTagsDto>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided fields parameter isn't valid. "
            );
        }

        HabitWithTagsDto habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ToHabitWithTagsDto())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound(new
            {
                message = $"Habit with ID '{id}' not found."
            });
        }

        ExpandoObject shapedHabitDto =
            dataShapingService.ShapeData(habit, fields);

        if (accept != CustomMediaTypeNames.Application.HateoasJson)
        {
            return Ok(shapedHabitDto);
        }

        List<LinkDto> links = CreateLinksForHabit(id, fields);
        shapedHabitDto.TryAdd("links", links);

        return Ok(shapedHabitDto);
    }



    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto  createHabitDto, IValidator<CreateHabitDto> validator)
    {

        await validator.ValidateAndThrowAsync(createHabitDto);

        Habit habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        HabitDto habitDto = habit.ToDto();
        habitDto.Links = CreateLinksForHabit(habitDto.Id, null);

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


    // V2
    [HttpGet("{id}")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetHabitV2(
        [FromRoute] string id,
        string? fields,
        [FromHeader(Name = "Accept")] string? accept,
        DataShapingService dataShapingService)
    {
        if (!dataShapingService.Validate<HabitWithTagsDtoV2>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided fields parameter isn't valid. "
            );
        }

        HabitWithTagsDtoV2 habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ToHabitWithTagsDtoV2())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound(new
            {
                message = $"Habit with ID '{id}' not found."
            });
        }

        ExpandoObject shapedHabitDto =
            dataShapingService.ShapeData(habit, fields);

        if (accept != CustomMediaTypeNames.Application.HateoasJson)
        {
            return Ok(shapedHabitDto);
        }

        List<LinkDto> links = CreateLinksForHabit(id, fields);
        shapedHabitDto.TryAdd("links", links);

        return Ok(shapedHabitDto);
    }







    private List<LinkDto> CreateLinksForHabits(
        HabitsQueryParameters parameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new
            {
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }
        return links;
    }

    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetHabit), "self", HttpMethods.Get, new { id, fields }),
            linkService.Create(nameof(UpdateHabit), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(PatchHabit), "partial-update", HttpMethods.Patch, new { id }),
            linkService.Create(nameof(DeleteHabit), "delete", HttpMethods.Delete, new { id }),
            linkService.Create(
                nameof(HabitTagsController.UpsertHabitTask), 
                "upsert-tags", 
                HttpMethods.Put, 
                new { habitId = id },
                HabitTagsController.Name),
        ];

        return links;
    }
}
