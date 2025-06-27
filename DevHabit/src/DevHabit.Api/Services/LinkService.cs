using DevHabit.Api.DTOs.Commom;

namespace DevHabit.Api.Services;

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
{
    public LinkDto Create(
        string endpointName,
        string rel,
        string method,
        object? values = null,
        string? controller = null
    )
    {
        string? href = linkGenerator.GetUriByAction(
            httpContextAccessor.HttpContext!,
            endpointName,
            controller: controller,
            values: values
        );

        return new LinkDto()
        {
            Href = href ?? throw new InvalidOperationException("Invalid endpoint name provided"),
            Rel = rel,
            Method = method
        };
    }
} 
