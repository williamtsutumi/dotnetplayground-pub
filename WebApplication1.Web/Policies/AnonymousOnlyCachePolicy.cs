using Microsoft.AspNetCore.OutputCaching;

namespace WebApplication1.Web.Policies;

public class AnonymousOnlyCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        bool anonymous =
            context.HttpContext.User.Identity?.IsAuthenticated != true;

        context.EnableOutputCaching = anonymous;
        context.AllowCacheLookup = anonymous;
        context.AllowCacheStorage = anonymous;
        context.AllowLocking = true;

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}