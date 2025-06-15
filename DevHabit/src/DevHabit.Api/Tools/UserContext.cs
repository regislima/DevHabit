using DevHabit.Api.Database;
using DevHabit.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DevHabit.Api.Tools;

public sealed class UserContext(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext dbContext,
    IMemoryCache memoryCache)
{
    private const string CacheKeyPrefix = "users:id";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var identityId = httpContextAccessor.HttpContext?.User.GetIdentityId();

        if (identityId is null)
            return null;

        string cahceKey = $"{CacheKeyPrefix}{identityId}";
        var userId = await memoryCache.GetOrCreateAsync(cahceKey, async entry =>
        {
            entry.SetSlidingExpiration(CacheDuration);
            
            return await dbContext.Users
                .Where(u => u.IdentityId == identityId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);
        });

        return userId;
    }
}
