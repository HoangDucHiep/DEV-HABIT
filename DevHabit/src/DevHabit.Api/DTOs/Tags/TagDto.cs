using DevHabit.Api.DTOs.Commom;
using DevHabit.Api.DTOs.Habits;

namespace DevHabit.Api.DTOs.Tags;


public sealed record TagsCollectionDto : ICollectionResponse<TagDto>
{
    public List<TagDto> Items { get; init; }
    public List<LinkDto> Links { get; set; } = new List<LinkDto>();
}

public class TagDto
{
    public required string Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public List<LinkDto> Links { get; set; } = new List<LinkDto>();
}
