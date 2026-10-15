using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApplication1.Application.Services.Usuario;

namespace WebApplication1.Web.Filters;

public sealed class UsuarioCookieEvents(IUsuarioService usuarios) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!int.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            || usuarios.Buscar(id) is null)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
