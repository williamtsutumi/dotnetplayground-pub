using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.Filters;

public class RegistrationConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var action in application.Controllers.Where(c => c.ControllerName == "Usuario")
                     .SelectMany(c => c.Actions).Where(a => a.ActionName == "Salvar"))
        {
            action.Filters.Add(new RegistrationExceptionFilter());
        }
    }
}

public class RegistrationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ArgumentException exception)
        {
            var localizer = context.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>>();
            context.Result = new BadRequestObjectResult(new { message = localizer[exception.Message].Value });
            context.ExceptionHandled = true;
        }
    }
}
