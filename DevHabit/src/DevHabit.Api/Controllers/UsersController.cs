using System.Security.Claims;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Entities;
using DevHabit.Api.Extensions;
using DevHabit.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize(Roles = Roles.Member)]
public class UsersController(
    ApplicationDbContext dbContext,
    UserContext userContext
    ) : ControllerBase
{
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        if (id != userId)
        {
            return Forbid();
        }

        UserDto? user = await dbContext.Users
            .Where(u => u.Id == id)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound(new
            {
                message = $"User with ID '{id}' not found."
            });
        }

        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                message = "User is not authenticated."
            });
        }

        UserDto? user = await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound(new
            {
                message = "Current user not found."
            });
        }

        return Ok(user);
    }
}
