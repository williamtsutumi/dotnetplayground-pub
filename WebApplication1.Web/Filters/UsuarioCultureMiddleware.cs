using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure;

namespace WebApplication1.Filters;

public sealed class UsuarioCultureMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requested = Normalize(context.Request.Query["culture"].ToString());
        var language = requested ?? Normalize(context.Request.Cookies["UsuarioCulture"]);
        language ??= context.Request.GetTypedHeaders().AcceptLanguage?
            .Where(value => (value.Quality ?? 1) > 0)
            .OrderByDescending(value => value.Quality ?? 1)
            .Select(value => NormalizeBrowserLanguage(value.Value.ToString()))
            .FirstOrDefault(value => value is not null);

        if (language is null && int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            var db = context.RequestServices.GetRequiredService<RedditDbContext>();
            language = Normalize(await db.Usuarios.Where(u => u.Id == id).Select(u => u.IdiomaPreferido)
                .SingleOrDefaultAsync(context.RequestAborted));
        }
        if (requested is not null)
            context.Response.Cookies.Append("UsuarioCulture", requested, new CookieOptions
            { HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Lax, MaxAge = TimeSpan.FromDays(365) });

        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language ?? "en");
            await next(context);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    private static string? NormalizeBrowserLanguage(string language) => language.Split('-')[0].ToLowerInvariant() switch
    {
        "pt" => "pt-BR",
        "en" => "en",
        _ => null
    };

    private static string? Normalize(string? language) => language?.ToUpperInvariant() switch
    {
        "PT" or "PTBR" or "PT-BR" => "pt-BR",
        "EN" or "EN-US" or "EN-GB" => "en",
        _ => null
    };
}
