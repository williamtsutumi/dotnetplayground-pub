using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Reflection;
using WebApplication1.Application.Services.AppGlobalSettings;

namespace WebApplication1.Controllers;

public class AboutController(
    IDeploymentInfoService deploymentInfoService,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.InstanceName = Environment.GetEnvironmentVariable("INSTANCE_NAME");
        if (string.IsNullOrEmpty(ViewBag.InstanceName))
            ViewBag.InstanceName = localizer["UnknownInstance"].Value;

        ViewBag.ExecutionUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
        if (string.IsNullOrEmpty(ViewBag.ExecutionUrl))
            ViewBag.ExecutionUrl = localizer["UnknownInstance"].Value;

        ViewBag.ExecutionUrl = ((string)ViewBag.ExecutionUrl).Split(':').Last();
        ViewBag.ApplicationVersion = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        try
        {
            ViewBag.LastDeploymentAt = await deploymentInfoService.ObterUltimoDeploy(cancellationToken);
        }
        catch (Exception)
        {
            ViewBag.LastDeploymentAt = null;
        }

        return View();
    }
}
