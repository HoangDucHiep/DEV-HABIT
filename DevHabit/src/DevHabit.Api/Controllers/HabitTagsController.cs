using DevHabit.Api.Database;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Authorize(Roles = Roles.Member)]
[Route("habits/{habitId}/tags")]
[ApiController]
public class HabitTagsController : ControllerBase
{
    public static readonly string Name = nameof(HabitTagsController).Replace("Controller", string.Empty);
    private readonly ApplicationDbContext dbContext;

    public HabitTagsController(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }


    [HttpPut]
    public async Task<ActionResult> UpsertHabitTask(string habitId, UpsertHabitTagsDto upsertHabitTagsDto)
    {
        Habit? habit = await dbContext.Habits
            .Include(h => h.HabitTags)
            .FirstOrDefaultAsync(h => h.Id == habitId);

        if (habit is null)
        {
            return NotFound(new
            {
                message = $"Habit with ID '{habitId}' not found."
            });
        }

        var currentTagIds = habit.HabitTags.Select(ht => ht.TagId).ToHashSet();

        if (currentTagIds.SetEquals(upsertHabitTagsDto.TagIds))
        {
            return NoContent();
        }

        List<string> existingTagIds = await dbContext
            .Tags
            .Where(t => upsertHabitTagsDto.TagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        if (existingTagIds.Count != upsertHabitTagsDto.TagIds.Count)
        {
            return BadRequest("One or more tag IDs is invalid");
        }

        habit.HabitTags.RemoveAll(ht => !upsertHabitTagsDto.TagIds.Contains(ht.TagId));

        string[] tagIdsToAdd = upsertHabitTagsDto.TagIds.Except(currentTagIds).ToArray();

        habit.HabitTags.AddRange(tagIdsToAdd.Select(tagId => new HabitTag
        {
            HabitId = habit.Id,
            TagId = tagId,
            CreateAtUtc = DateTime.UtcNow
        }));

        await dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteHabitTask(string habitId, string tagId)
    {
        HabitTag? habitTag = await dbContext.HabitTags
            .SingleOrDefaultAsync(ht => ht.HabitId == habitId && ht.TagId == tagId);

        if (habitTag is null)
        {
            return NotFound(new
            {
                message = $"HabitTag with Habit ID '{habitId}' and Tag ID '{tagId}' not found."
            });
        }

        dbContext.HabitTags.Remove(habitTag);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
