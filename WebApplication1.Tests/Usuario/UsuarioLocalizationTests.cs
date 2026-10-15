using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using WebApplication1.Resources;

namespace WebApplication1.Tests.Usuario;

public class UsuarioLocalizationTests
{
    [Theory]
    [InlineData("pt")]
    [InlineData("pt-BR")]
    [InlineData("pt-PT,pt;q=0.9,en;q=0.8")]
    public async Task AuthenticatedVisitUsesPortugueseBrowserBeforeAccountDefault(string acceptedLanguages)
    {
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        context.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") },
                "Cookies"));
        context.Request.Headers.AcceptLanguage = acceptedLanguages;
        // No database services: a supported browser language must resolve without consulting the account.
        var middleware = new WebApplication1.Filters.UsuarioCultureMiddleware(_ =>
        {
            Assert.Equal("pt-BR", CultureInfo.CurrentCulture.Name);
            Assert.Equal("pt-BR", CultureInfo.CurrentUICulture.Name);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);
    }

    [Theory]
    [InlineData("pt-BR,pt;q=0.9,en;q=0.8", "pt-BR")]
    [InlineData("pt-PT", "pt-BR")]
    [InlineData("en-AU", "en")]
    [InlineData("en;q=0.5,pt-BR;q=0.9", "pt-BR")]
    [InlineData("fr-FR,pt;q=0.8,en;q=0.5", "pt-BR")]
    [InlineData("pt;q=0,en;q=0.5", "en")]
    [InlineData("fr-FR", "en")]
    [InlineData("", "en")]
    public async Task FirstVisitUsesSupportedBrowserLanguage(string acceptedLanguages, string expected)
    {
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        context.Request.Headers.AcceptLanguage = acceptedLanguages;
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        var middleware = new WebApplication1.Filters.UsuarioCultureMiddleware(_ =>
        {
            Assert.Equal(expected, CultureInfo.CurrentCulture.Name);
            Assert.Equal(expected, CultureInfo.CurrentUICulture.Name);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.Empty(context.Response.Headers.SetCookie.ToString());
        Assert.Equal(previous, CultureInfo.CurrentCulture);
        Assert.Equal(previousUi, CultureInfo.CurrentUICulture);
    }

    [Theory]
    [InlineData("", "UsuarioCulture=en", "pt-BR", "en")]
    [InlineData("?culture=pt-BR", "UsuarioCulture=en", "en-US", "pt-BR")]
    [InlineData("?culture=invalid", "UsuarioCulture=invalid", "pt-BR", "pt-BR")]
    public async Task ExplicitChoiceTakesPrecedenceOverBrowserLanguage(
        string query, string cookie, string acceptedLanguages, string expected)
    {
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        context.Request.QueryString = new Microsoft.AspNetCore.Http.QueryString(query);
        context.Request.Headers.Cookie = cookie;
        context.Request.Headers.AcceptLanguage = acceptedLanguages;
        var middleware = new WebApplication1.Filters.UsuarioCultureMiddleware(_ =>
        {
            Assert.Equal(expected, CultureInfo.CurrentUICulture.Name);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);
    }

    [Theory]
    [InlineData("en", "Welcome back.", "Invalid username or password.")]
    [InlineData("pt-BR", "Boas-vindas de volta.", "Usuário ou senha inválidos.")]
    public void TypedLocalizerLoadsExistingResourceFiles(string culture, string welcome, string error)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddTransient<IStringLocalizer<SharedResource>, UsuarioLocalizer>();
        using var provider = services.BuildServiceProvider();
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            var localizer = provider.GetRequiredService<IStringLocalizer<SharedResource>>();
            Assert.Equal(welcome, localizer["LoginWelcomeHeading"].Value);
            Assert.Equal(error, localizer["InvalidLogin"].Value);
            Assert.False(localizer["RegistrationSuccess"].ResourceNotFound);
            Assert.Equal(culture == "en" ? "Home feed" : "Feed principal", localizer["MainFeed"].Value);
            Assert.Equal(culture == "en" ? "Reply to Ana" : "Responder a Ana", localizer["PostDetailsReplyToAccessibleLabel", "Ana"].Value);
            Assert.True(localizer.GetAllStrings().Count() >= 30);
        }
        finally { CultureInfo.CurrentUICulture = previous; }
    }

    [Fact]
    public void BothLanguagesHaveTheSameKeysAndFormatArguments()
    {
        var assembly = typeof(SharedResource).Assembly;
        var english = new System.Resources.ResourceManager("WebApplication1.Web.Resources.resource_en", assembly);
        var portuguese = new System.Resources.ResourceManager("WebApplication1.Web.Resources.resource_ptbr", assembly);
        using var en = english.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        using var pt = portuguese.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        var enValues = en.Cast<System.Collections.DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);
        var ptValues = pt.Cast<System.Collections.DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);
        Assert.Equal(enValues.Keys.OrderBy(k => k), ptValues.Keys.OrderBy(k => k));
        foreach (var (key, value) in enValues)
        {
            Assert.False(string.IsNullOrWhiteSpace(value));
            var pattern = @"\{\d+\}";
            Assert.Equal(System.Text.RegularExpressions.Regex.Matches(value, pattern).Select(m => m.Value).OrderBy(v => v),
                System.Text.RegularExpressions.Regex.Matches(ptValues[key], pattern).Select(m => m.Value).OrderBy(v => v));
        }
    }

    [Theory]
    [InlineData("Post", "pt-BR")]
    [InlineData("Comunidade", "en")]
    [InlineData("About", "pt-BR")]
    public async Task LanguageChoiceAppliesToRemainingRoutesAndIsRemembered(string controller, string culture)
    {
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        context.Request.RouteValues["controller"] = controller;
        context.Request.QueryString = new Microsoft.AspNetCore.Http.QueryString("?culture=" + culture);
        var previous = CultureInfo.CurrentUICulture;
        var middleware = new WebApplication1.Filters.UsuarioCultureMiddleware(_ =>
        {
            Assert.Equal(culture, CultureInfo.CurrentUICulture.Name);
            Assert.Equal(culture, CultureInfo.CurrentCulture.Name);
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context);
        Assert.Contains("UsuarioCulture=" + culture, context.Response.Headers.SetCookie.ToString());
        Assert.Equal(previous, CultureInfo.CurrentUICulture);
    }
}
