using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Serilog;
using System.Reflection;
using WebApplication1.Application.Repositories;
using WebApplication1.Application.Services;
using WebApplication1.Application.Services.AppGlobalSettings;
using WebApplication1.Application.Services.Comunidade;
using WebApplication1.Application.Services.Feed;
using WebApplication1.Application.Services.Post;
using WebApplication1.Application.Services.Usuario;
using WebApplication1.Conventions;
using WebApplication1.Infrastructure;
using WebApplication1.Infrastructure.Repositories;
using WebApplication1.Resources;
using WebApplication1.Web.Filters;
using WebApplication1.Web.Mappers;
using WebApplication1.Web.Policies;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("/var/webapp/logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30, shared: true));


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddTransient<IStringLocalizer<WebApplication1.SharedResource>, UsuarioLocalizer>();
builder.Services.AddTransient<IConfigureOptions<MvcOptions>, LocalizedModelBinding>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Conventions.Add(new WebApplication1.Filters.RegistrationConvention());
    options.Conventions.Add(new AdminAuthorizationConvention());

}).AddDataAnnotationsLocalization(options => options.DataAnnotationLocalizerProvider = (_, factory) => new UsuarioLocalizer(factory));


builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IComunidadeRepository, ComunidadeRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IAppGlobalSettingsRepository, AppGlobalSettingsRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();

builder.Services.AddScoped<IComunidadeService, ComunidadeService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<UsuarioCookieEvents>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IDeploymentInfoService, DeploymentInfoService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddAutoMapper(
    cfg => { },
    typeof(UsuarioProfile),
    typeof(ComunidadeProfile),
    typeof(FeedProfile),
    typeof(PerfilProfile)
);

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy(
        "AnonymousOnly",
        new AnonymousOnlyCachePolicy());
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuario/Login";
        options.AccessDeniedPath = "/Usuario/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.EventsType = typeof(UsuarioCookieEvents);
    });

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/webapp/keys"))
    .SetApplicationName("WebApplication1");

builder.Services.AddAuthorization();

builder.Services.AddDbContext<RedditDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

var applicationVersion = Assembly.GetEntryAssembly()?
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
if (!string.IsNullOrWhiteSpace(applicationVersion))
{
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var deploymentInfoService = scope.ServiceProvider.GetRequiredService<IDeploymentInfoService>();
        deploymentInfoService.RegistrarVersaoAtual(applicationVersion);
        app.Logger.LogInformation(
            "Deployment metadata updated. Version: {ApplicationVersion}; timestamp: {DeploymentTimestamp}.",
            applicationVersion,
            DateTimeOffset.UtcNow);
    }
    catch (Exception exception)
    {
        app.Logger.LogError(exception, "Could not update deployment metadata during application startup.");
    }
}
else
{
    app.Logger.LogWarning("Assembly informational version is missing; deployment timestamp was not updated.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<WebApplication1.Filters.UsuarioCultureMiddleware>();
app.UseAuthorization();
app.UseOutputCache();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
