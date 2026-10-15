using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure;
using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace WebApplication1.Controllers;

[AllowAnonymous]
public class SearchController : Controller
{
    private IStringLocalizer<SharedResource> Localizer => HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

    private readonly RedditDbContext _db;
    private readonly ILogger<SearchController> _logger;

    public SearchController(RedditDbContext db, ILogger<SearchController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? query, string? tipo = "posts")
    {
        ViewData["PostContentReduced"] = true;

        var termo = query?.Trim() ?? string.Empty;
        var filtro = NormalizarTipo(tipo);
        var model = new SearchPageModel { Query = termo, Tipo = filtro };

        if (string.IsNullOrWhiteSpace(termo))
            return View(model);

        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var leitorId);
        var leitor = leitorId > 0 ? leitorId : (int?)null;

        try
        {
            var postsQuery = FeedQueries.Posts(_db, leitorId: leitor)
                .Where(p => p.Titulo.Contains(termo) || p.Conteudo.Contains(termo));
            var comunidadesQuery = _db.Comunidades.AsNoTracking()
                .Where(c => c.Nome.Contains(termo));
            var usuariosQuery = _db.Usuarios.AsNoTracking()
                .Where(u => u.NomeUsuario.Contains(termo));

            model.TotalPosts = await postsQuery.CountAsync(HttpContext.RequestAborted);
            model.TotalComunidades = await comunidadesQuery.CountAsync(HttpContext.RequestAborted);
            model.TotalUsuarios = await usuariosQuery.CountAsync(HttpContext.RequestAborted);

            if (filtro == "posts")
            {
                model.Posts = await postsQuery
                    .OrderByDescending(p => p.CriadoEm)
                    .Take(100)
                    .ToListAsync(HttpContext.RequestAborted);
            }
            else if (filtro == "comunidades")
            {
                model.Comunidades = await comunidadesQuery
                    .OrderBy(c => c.Nome)
                    .Take(100)
                    .ToListAsync(HttpContext.RequestAborted);
            }
            else
            {
                model.Usuarios = await usuariosQuery
                    .OrderBy(u => u.NomeUsuario)
                    .Select(u => new SearchUsuarioModel { Id = u.Id, NomeUsuario = u.NomeUsuario })
                    .Take(100)
                    .ToListAsync(HttpContext.RequestAborted);
            }
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Falha ao buscar por {Query}", termo);
            model.Indisponivel = true;
        }

        return View(model);
    }

    private static string NormalizarTipo(string? tipo) =>
        tipo?.Trim().ToLowerInvariant() switch
        {
            "comunidades" => "comunidades",
            "usuarios" or "usuários" or "users" => "usuarios",
            _ => "posts"
        };
}
