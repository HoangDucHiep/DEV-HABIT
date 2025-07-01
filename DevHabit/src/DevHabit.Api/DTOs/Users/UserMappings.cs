using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.Entities;
using DevHabit.Api.Utils;

namespace DevHabit.Api.DTOs.Users;

public static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto)
    {
        return new User
        {
            Id = IdGenerator.GenerateId(IdPrefix.USER),
            Name = dto.Name,
            Email = dto.Email,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
