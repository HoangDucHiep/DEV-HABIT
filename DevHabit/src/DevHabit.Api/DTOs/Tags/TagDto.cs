using DevHabit.Api.DTOs.Habits;

namespace DevHabit.Api.DTOs.Tags;


public sealed record TagsCollectionDto
{
    public List<TagDto> Data { get; init; }
}

public class TagDto
{
    public required string Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
