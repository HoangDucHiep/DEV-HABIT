using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;


[ApiController]
[Route("tags")]
public class TagController : ControllerBase
{
    private readonly ApplicationDbContext dbContext;
    public TagController(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags()
    {
        List<TagDto> tags = await dbContext.
            Tags
            .Select(TagQueries.ToTagDto())
            .ToListAsync();

        var tagsCollection = new TagsCollectionDto
        {
            Items = tags
        };

        return Ok(tagsCollection);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag([FromRoute] string id)
    {
        TagDto? tag = await dbContext
            .Tags
            .Where(t => t.Id == id)
            .Select(TagQueries.ToTagDto())
            .FirstOrDefaultAsync();
        if (tag is null)
        {
            return NotFound(new
            {
                message = $"Tag with ID '{id}' not found."
            });
        }
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto, IValidator<CreateTagDto> validator)
    {
        // validate the DTO
        await validator.ValidateAndThrowAsync(createTagDto);

        Tag tag = createTagDto.ToEntity();

        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return Problem(
                detail: $"The tag '{tag.Name}' already exists",
                statusCode: StatusCodes.Status409Conflict);
        }

        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        TagDto tagDto = tag.ToDto();

        return CreatedAtAction(nameof(GetTag), new { id = tagDto.Id }, tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, UpdateTagDto updateTagDto)
    {
        Tag? tag = await dbContext
            .Tags
            .FirstOrDefaultAsync(h => h.Id == id);

        if (tag is null)
        {
            return NotFound(new
            {
                message = $"Tag with ID '{id}' not found."
            });
        }

        tag.UpdateFromDto(updateTagDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        Tag? tag = await dbContext
            .Tags
            .FirstOrDefaultAsync(h => h.Id == id);
        if (tag is null)
        {
            return NotFound(new
            {
                message = $"Tag with ID '{id}' not found."
            });
        }
        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

      
}
