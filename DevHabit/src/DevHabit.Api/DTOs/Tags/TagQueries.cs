using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

public static class TagQueries
{
    public static System.Linq.Expressions.Expression<Func<Tag, TagDto>> ToTagDto()
    {
        return tag => new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc
        };
    }
}
