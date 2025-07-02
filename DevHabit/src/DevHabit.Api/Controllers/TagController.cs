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

[Authorize(Roles = Roles.Member)]
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
    private readonly UserContext userContext;

    public TagController(ApplicationDbContext dbContext, LinkService linkService, UserContext userContext)
    {
        this.dbContext = dbContext;
        this.linkService = linkService;
        this.userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags([FromHeader] AcceptHeaderDto acceptHeader)
    {
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        List<TagDto> tags = await dbContext.
            Tags
            .Where(t => t.UserId == userId)
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
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        TagDto? tag = await dbContext
            .Tags
            .Where(t => t.Id == id && t.UserId == userId)
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
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        // validate the DTO
        await validator.ValidateAndThrowAsync(createTagDto);

        Tag tag = createTagDto.ToEntity(userId);

        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name && t.UserId == userId))
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
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        Tag? tag = await dbContext
            .Tags
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

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
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        Tag? tag = await dbContext
            .Tags
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

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
