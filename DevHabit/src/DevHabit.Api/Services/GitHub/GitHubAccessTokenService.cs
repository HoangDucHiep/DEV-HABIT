using DevHabit.Api.Database;
using DevHabit.Api.DTOs.GitHub;
using DevHabit.Api.Entities;
using DevHabit.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Services.GitHub;

public sealed class GitHubAccessTokenService (
    ApplicationDbContext dbContext,
    EncryptionService encryptionService
    )
{

    public async Task StoreAsync(
        string userId,
        StoreGithubAccessTokenDto accessTokenDto,
        CancellationToken cancellationToken = default
    )
    {
        GitHubAccessToken? existingAccessToken = await GetAccessTokenAsync(userId, cancellationToken);

        string encryptedToken = encryptionService.Encrypt(accessTokenDto.AccessToken);

        if (existingAccessToken is not null)
        {
            existingAccessToken.Token = encryptedToken;
            existingAccessToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(accessTokenDto.ExpiresInDays);
        }
        else
        {
            dbContext.GitHubAccessTokens.Add(new GitHubAccessToken
            {
                Id = IdGenerator.GenerateId(IdPrefix.GITHUB_ACCESS_TOKEN),
                UserId = userId,
                Token = encryptedToken,
                CreateAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(accessTokenDto.ExpiresInDays)
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        GitHubAccessToken? accessToken = await GetAccessTokenAsync(userId, cancellationToken);

        if (accessToken is null)
        {
            return null;
        }

        string decryptedToken = encryptionService.Decrypt(accessToken.Token);

        return decryptedToken;
    }

    public async Task RevokeAsync(string userId, CancellationToken cancellationToken = default)
    {
        GitHubAccessToken? accessToken = await GetAccessTokenAsync(userId, cancellationToken);

        if (accessToken is null)
        {
            return;
        }


        dbContext.GitHubAccessTokens.Remove(accessToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<GitHubAccessToken?> GetAccessTokenAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.GitHubAccessTokens
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

}
