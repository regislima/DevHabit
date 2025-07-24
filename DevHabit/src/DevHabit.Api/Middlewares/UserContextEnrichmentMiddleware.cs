using System.Diagnostics;
using DevHabit.Api.Services;
using DevHabit.Api.Tools;

namespace DevHabit.Api.Middlewares;

public sealed class UserContextEnrichmentMiddleware(
    RequestDelegate next,
    ILogger<UserContextEnrichmentMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, UserContext userContext)
    {
        string? userId = await userContext.GetUserIdAsync();

        if (userId is not null)
        {
            Activity.Current?.SetTag("user.id", userId);

            using (logger.BeginScope(
                       new Dictionary<string, object>
                       {
                           ["UserId"] = userId
                       }))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }
}
