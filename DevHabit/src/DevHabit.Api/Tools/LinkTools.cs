using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.Tools;

public sealed class LinkTools(
    LinkGenerator linkGenerator,
    IHttpContextAccessor httpContextAccessor)
{
    public LinkDto Create(
        string endpointName,
        string rel,
        string method,
        object? values = null,
        string? controller = null)
    {
        var href = linkGenerator.GetUriByAction(
            httpContextAccessor.HttpContext!,
            endpointName,
            controller,
            values);

        return new LinkDto
        {
            Href = href ?? throw new InvalidOperationException("Invalid endpoint name provided."),
            Rel = rel,
            Method = method
        };
    }
}
