using DevHabit.Api.Entities;
using DevHabit.Api.Utils;

namespace DevHabit.Api.DTOs.Tags;

public static class TagMappings
{
    public static Tag ToEntity(this CreateTagDto dto)
    {
        Tag tag = new()
        {
            Id = IdGenerator.GenerateId(IdPrefix.TAG),
            Name = dto.Name,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow,
        };

        return tag;
    }

    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto()
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc
        };
    }

    public static void UpdateFromDto(this Tag tag, UpdateTagDto dto)
    {
        tag.Name = dto.Name;
        tag.Description = dto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }
}
