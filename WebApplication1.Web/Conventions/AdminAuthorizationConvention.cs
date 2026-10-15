using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace WebApplication1.Conventions;

public class AdminAuthorizationConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var controllerNamespace = controller.ControllerType.Namespace;
        var isAdminNamespace = controllerNamespace is not null
            && controllerNamespace.StartsWith(
                "WebApplication1.Areas.Admin.Controllers",
                StringComparison.OrdinalIgnoreCase);

        if (isAdminNamespace && !controller.RouteValues.ContainsKey("area"))
        {
            controller.RouteValues["area"] = "Admin";
        }

        var isAdminArea = controller.RouteValues.TryGetValue("area", out var area)
            && string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase);

        if (!isAdminArea)
        {
            return;
        }

        var policy = new AuthorizationPolicyBuilder()
            .RequireRole("Admin")
            .Build();

        controller.Filters.Add(new AuthorizeFilter(policy));
    }
}
