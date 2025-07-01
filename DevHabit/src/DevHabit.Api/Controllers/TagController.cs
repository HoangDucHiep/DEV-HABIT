using System.Net.Mime;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Commom;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Authorize]
[ApiController]
[Route("tags")]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1)]
public class TagController : ControllerBase
{
    private readonly ApplicationDbContext dbContext;
    private readonly LinkService linkService;

    public TagController(ApplicationDbContext dbContext, LinkService linkService)
    {
        this.dbContext = dbContext;
        this.linkService = linkService;
    }

    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags([FromHeader] AcceptHeaderDto acceptHeader)
    {
        List<TagDto> tags = await dbContext.
            Tags
            .Select(TagQueries.ToTagDto())
            .ToListAsync();

        // Add links to each individual tag if requested
        if (acceptHeader.IncludeLinks)
        {
            foreach (TagDto tag in tags)
            {
                tag.Links = CreateLinksForTag(tag.Id);
            }
        }

        var tagsCollection = new TagsCollectionDto
        {
            Items = tags
        };

        if (acceptHeader.IncludeLinks)
        {
            tagsCollection.Links = CreateLinksForTags();
        }

        return Ok(tagsCollection);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag([FromRoute] string id, [FromHeader] AcceptHeaderDto acceptHeader)
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

        if (acceptHeader.IncludeLinks)
        {
            tag.Links = CreateLinksForTag(id);
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


    private List<LinkDto> CreateLinksForTags()
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetTags), "self", HttpMethods.Get),
            linkService.Create(nameof(CreateTag), "create", HttpMethods.Post)
        ];

        return links;
    }

    private List<LinkDto> CreateLinksForTag(string id)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetTag), "self", HttpMethods.Get, new { id }),
            linkService.Create(nameof(UpdateTag), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(DeleteTag), "delete", HttpMethods.Delete, new { id })
        ];

        return links;
    }
}
