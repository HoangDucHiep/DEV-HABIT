using System.Security.Claims;

namespace DevHabit.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetIdentityId(this ClaimsPrincipal? user)
    {
        string? identityId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        return identityId;
    }
}
