using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure;
using WebApplication1.Models.Comunidade;

namespace WebApplication1.ViewComponents;

public class SidebarComunidadesViewComponent : ViewComponent
{
    private readonly RedditDbContext _db;
    private readonly ILogger<SidebarComunidadesViewComponent> _logger;

    public SidebarComunidadesViewComponent(RedditDbContext db, ILogger<SidebarComunidadesViewComponent> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new SidebarComunidadesModel { Autenticado = User.Identity?.IsAuthenticated == true };
        if (!model.Autenticado)
            return View(model);

        if (!int.TryParse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
        {
            model.Indisponivel = true;
            return View(model);
        }

        try
        {
            model.Comunidades = await _db.Comunidades.AsNoTracking()
                .Where(c => c.IdCriadorUsuario == usuarioId ||
                    _db.ComunidadeUsuarios.Any(m => m.IdComunidade == c.Id && m.IdUsuario == usuarioId))
                .OrderBy(c => c.Nome)
                .Select(c => new SidebarComunidadeItem
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    CriadaPorMim = c.IdCriadorUsuario == usuarioId
                }).ToListAsync(HttpContext.RequestAborted);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Não foi possível carregar as comunidades da sidebar.");
            model.Indisponivel = true;
        }
        return View(model);
    }
}
