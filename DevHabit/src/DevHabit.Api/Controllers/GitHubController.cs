using DevHabit.Api.DTOs.Commom;
using DevHabit.Api.DTOs.GitHub;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Services.GitHub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Controllers;

[Authorize(Roles = Roles.Member)]
[ApiController]
[Route("github")]
public sealed class GitHubController(
    GitHubAccessTokenService gitHubAccessTokenService,
    GitHubSerive gitHubSerive,
    UserContext userContext,
    LinkService linkService
    ) : ControllerBase
{
    [HttpPut("personal-access-token")]
    public async Task<IActionResult> StoreAccessToken(
        StoreGithubAccessTokenDto storeGithubAccessTokenDto
    )
    {
        string? userId = await userContext.GetUserIdAsync();
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await gitHubAccessTokenService.StoreAsync(userId, storeGithubAccessTokenDto);
        return NoContent();
    }

    [HttpDelete("personal-access-token")]
    public async Task<IActionResult> RevokeAccessToken()
    {
        string? userId = await userContext.GetUserIdAsync();
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        await gitHubAccessTokenService.RevokeAsync(userId);
        return NoContent();
    }

    [HttpGet("profile")]
    public async Task<ActionResult<GitHubUserProfileDto>> GetUserProfile([FromHeader] AcceptHeaderDto acceptHeader)
    {
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "User not authenticated or user ID not found."
            });
        }

        string? accessToken = await gitHubAccessTokenService.GetAsync(userId);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return NotFound();
        }

        GitHubUserProfileDto? userProfile = await gitHubSerive.GetUserProfileAsync(accessToken);

        if (userProfile is null)
        {
            return NotFound(new
            {
                message = "GitHub user profile not found."
            });
        }

        if (acceptHeader.IncludeLinks)
        {
            // Create a new instance of GitHubUserProfileDto with updated Links
            GitHubUserProfileDto updatedUserProfile = userProfile with
            {
                Links = new List<LinkDto>
                {
                    linkService.Create(nameof(GetUserProfile), "self", HttpMethods.Get),
                    linkService.Create(nameof(StoreAccessToken), "store-token", HttpMethods.Put),
                    linkService.Create(nameof(RevokeAccessToken), "revoke-token", HttpMethods.Delete),
                },
            };

            return Ok(updatedUserProfile);
        }
        
        return Ok(userProfile);
    }
}
