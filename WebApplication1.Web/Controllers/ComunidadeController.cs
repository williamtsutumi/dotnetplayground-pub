using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using WebApplication1.Application.Records;
using WebApplication1.Application.Services.Comunidade;
using WebApplication1.Infrastructure;
using WebApplication1.Models.Comunidade;

namespace WebApplication1.Controllers;

public class ComunidadeController : Controller
{
    private IStringLocalizer<SharedResource> Localizer => HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

    private readonly IComunidadeService _comunidadeService;
    private readonly IMapper _mapper;

    private readonly RedditDbContext _db;
    private readonly ILogger<ComunidadeController> _logger;

    public ComunidadeController(RedditDbContext db, ILogger<ComunidadeController> logger, IComunidadeService service, IMapper mapper)
    {
        _db = db;
        _logger = logger;
        _comunidadeService = service;
        _mapper = mapper;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? busca)
    {
        ViewData["Busca"] = busca;
        try
        {
            var query = _db.Comunidades.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(c => c.Nome.Contains(busca.Trim()));
            return View(await query.OrderBy(c => c.Nome).Take(100).ToListAsync());
        }
        catch (NpgsqlException ex)
        {
            DatabaseError(ex);
            return View(new List<Domain.Comunidade.Comunidade>());
        }
    }

    [AllowAnonymous]
    public async Task<IActionResult> Detalhes(int id)
    {
        return await View(id);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Criar(int id)
    {
        if (id > 0)
        {
            var comunidade = _comunidadeService.BuscarPorId(id);
            return View(new CriarComunidadeRequest()
            {
                Id = comunidade.Id,
                Nome = comunidade.Nome,
                Descricao = comunidade.Descricao
            });
        }

        return View(new CriarComunidadeRequest());
    }

    [HttpPost]
    public IActionResult Criar(CriarComunidadeRequest criarComunidadeDTO)
    {
        if (!ModelState.IsValid) return View(criarComunidadeDTO);

        var cmd = _mapper.Map<CriarComunidadeResponse>(criarComunidadeDTO);
        cmd.UsuarioNome = User.Identity!.Name;
        cmd.UsuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        var resultado = _comunidadeService.Salvar(cmd);

        if (!resultado.Sucesso)
        {
            ModelState.AddModelError(nameof(criarComunidadeDTO.Nome), Localizer[resultado.CodigoErro!].Value);
            return View(criarComunidadeDTO);
        }

        TempData["Sucesso"] = Localizer["CommunityCreateSuccessNotice"].Value;
        return RedirectToAction(nameof(View), new { id = resultado.Valor });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var comunidade = await _db.Comunidades.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            if (comunidade is null) return NotFound();
            if (await CurrentUserId() != comunidade.IdCriadorUsuario) return Forbid();
            return View(new ComunidadeFormModel { Nome = comunidade.Nome, Descricao = comunidade.Descricao });
        }
        catch (NpgsqlException ex) { DatabaseError(ex); return View("Indisponivel"); }
    }

    [HttpPost]
    public async Task<IActionResult> Editar(int id, ComunidadeFormModel model)
    {
        try
        {
            var comunidade = await _db.Comunidades.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            if (comunidade is null) return NotFound();
            var userId = await CurrentUserId();
            if (userId != comunidade.IdCriadorUsuario) return Forbid();
            if (!ModelState.IsValid) return View(model);
            if (await NameExists(model.Nome, id))
            {
                ModelState.AddModelError(nameof(model.Nome), Localizer["CommunityNameAlreadyExistsError"].Value);
                return View(model);
            }
            // Nome is an alternate key in the existing schema; update without tracking that key.
            var affected = await _db.Comunidades.Where(c => c.Id == id && c.IdCriadorUsuario == userId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Nome, model.Nome)
                    .SetProperty(c => c.Descricao, model.Descricao == null ? null : model.Descricao.Trim()));
            if (affected == 0) return NotFound();
            TempData["Sucesso"] = Localizer["CommunityUpdateSuccessNotice"].Value;
            return RedirectToAction(nameof(Detalhes), new { id });
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { ModelState.AddModelError(nameof(model.Nome), Localizer["CommunityNameAlreadyExistsError"].Value); }
        catch (NpgsqlException ex) { DatabaseError(ex); }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            var comunidade = await _db.Comunidades.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            if (comunidade is null) return NotFound();
            if (await CurrentUserId() != comunidade.IdCriadorUsuario) return Forbid();
            return View(comunidade);
        }
        catch (NpgsqlException ex) { DatabaseError(ex); return View("Indisponivel"); }
    }

    [HttpPost, ActionName("Excluir")]
    public async Task<IActionResult> ExcluirConfirmado(int id)
    {
        Domain.Comunidade.Comunidade? comunidade = null;
        try
        {
            comunidade = await _db.Comunidades.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            if (comunidade is null) return NotFound();
            var userId = await CurrentUserId();
            if (userId != comunidade.IdCriadorUsuario) return Forbid();
            if (await _db.Posts.AnyAsync(p => p.ComunidadeId == id) || await _db.ComunidadeUsuarios.AnyAsync(m => m.IdComunidade == id))
            {
                ModelState.AddModelError("", Localizer["CommunityDeleteBlockedContentError"].Value);
                return View("Excluir", comunidade);
            }
            var affected = await _db.Comunidades.Where(c => c.Id == id && c.IdCriadorUsuario == userId).ExecuteDeleteAsync();
            if (affected == 0) return NotFound();
            TempData["Sucesso"] = Localizer["CommunityDeleteSuccessNotice"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        { ModelState.AddModelError("", Localizer["CommunityDeleteBlockedRelationsError"].Value); }
        catch (NpgsqlException ex) { DatabaseError(ex); }
        return comunidade is null ? View("Indisponivel") : View("Excluir", comunidade);
    }

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> View(int id, int pagina = 1)
    {
        try
        {
            var comunidade = await _db.Comunidades.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            if (comunidade is null) return NotFound();
            var userId = await CurrentUserId();
            pagina = Math.Clamp(pagina, 1, 100000);
            var posts = await (from p in _db.Posts.AsNoTracking()
                               join usuario in _db.Usuarios on p.IdAutorUsuario equals usuario.Id into autores
                               from u in autores.DefaultIfEmpty()
                               where p.ComunidadeId == id
                               orderby p.CriadoEm descending, p.Id descending
                               select new ComunidadePostModel {
                                   Id = p.Id,
                                   AutorId = u == null ? 0 : u.Id,
                                   TotalComentarios = _db.Comentarios.Count(c => c.IdPost == p.Id),
                                   Upvotes = _db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == Domain.UsuarioUpvotePost.TipoVoto.UpVote)
                                       - _db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == Domain.UsuarioUpvotePost.TipoVoto.DownVote),
                                   MeuUpvote = _db.UsuarioUpvotePosts.Any(v => v.IdPost == p.Id && v.IdUsuario == userId && v.Tipo == Domain.UsuarioUpvotePost.TipoVoto.UpVote),
                                   MeuDownvote = _db.UsuarioUpvotePosts.Any(v => v.IdPost == p.Id && v.IdUsuario == userId && v.Tipo == Domain.UsuarioUpvotePost.TipoVoto.DownVote),
                                   Titulo = p.Titulo,
                                   Conteudo = p.Conteudo,
                                   ConteudoMarkDown = p.ConteudoMarkdown,
                                   Autor = u == null ? string.Empty : u.NomeUsuario,
                                   CriadoEm = p.CriadoEm
                               })
                .Skip((pagina - 1) * 20).Take(21).ToListAsync();
            return View("View", new ComunidadePaginaModel
            {
                Comunidade = comunidade,
                Criador =
                    await _db.Usuarios
                        .Where(u => u.Id == comunidade.IdCriadorUsuario)
                        .Select(u => u == null ? string.Empty : u.NomeUsuario)
                        .SingleOrDefaultAsync() ?? string.Empty,
                PodeGerenciar = userId.HasValue && userId == comunidade.IdCriadorUsuario,
                Participa = userId.HasValue && userId == comunidade.IdCriadorUsuario
                    || await _db.ComunidadeUsuarios.AnyAsync(m => m.IdComunidade == id && m.IdUsuario == userId),
                Membros = 
                    await _db.ComunidadeUsuarios
                        .Where(m => m.IdComunidade == id)
                        .Select(m => m.IdUsuario)
                        .Union(_db.Comunidades.Where(c => c.Id == id && c.IdCriadorUsuario.HasValue).Select(c => c.IdCriadorUsuario!.Value))
                        .CountAsync(),
                Pagina = pagina, TemProxima = posts.Count > 20,
                Posts = posts.Take(20).ToList()
            });
        }
        catch (NpgsqlException ex) { DatabaseError(ex); return View("Indisponivel"); }
    }

    [HttpPost]
    public async Task<IActionResult> Unir(int id)
    {
        try
        {
            var userId = await CurrentUserId();
            if (userId is null) return Challenge();
            var comunidade = await _db.Comunidades.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            if (comunidade is null) return NotFound();
            if (comunidade.IdCriadorUsuario != userId && !await _db.ComunidadeUsuarios.AnyAsync(m => m.IdComunidade == id && m.IdUsuario == userId))
            {
                _db.ComunidadeUsuarios.Add(new Domain.ComunidadeUsuario.ComunidadeUsuario
                { IdComunidade = id, IdUsuario = userId.Value, Papel = 0, EntrouEm = DateTime.UtcNow });
                await _db.SaveChangesAsync();
            }
            TempData["Sucesso"] = Localizer["CommunityJoinSuccessNotice"].Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { TempData["Sucesso"] = Localizer["CommunityJoinAlreadyMemberNotice"].Value; }
        catch (Exception ex) when (ex is NpgsqlException or DbUpdateException)
        { _logger.LogError(ex, "Falha ao participar da comunidade."); TempData["Erro"] = Localizer["CommunityJoinFailureNotice"].Value; }
        return RedirectToAction("View", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Postar(int id)
    {
        try
        {
            var comunidade = await _db.Comunidades.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            if (comunidade is null) return NotFound();
            return View(new CriarPostModel { ComunidadeId = id, ComunidadeNome = comunidade.Nome });
        }
        catch (NpgsqlException ex) { DatabaseError(ex); return View("Indisponivel"); }
    }

    [HttpPost]
    public async Task<IActionResult> Postar(int id, CriarPostModel model)
    {
        try
        {
            var userId = await CurrentUserId();
            if (userId is null) return Challenge();
            var comunidade = await _db.Comunidades.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            if (comunidade is null) return NotFound();
            model.ComunidadeId = id;
            model.ComunidadeNome = comunidade.Nome;
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = Localizer["CommunityPostCreateFieldsError"].Value;
                return View(model);
            }
            _db.Posts.Add(new Domain.Post.Post { ComunidadeId = id, IdAutorUsuario = userId.Value,
                Titulo = model.Titulo.Trim(), Conteudo = model.ConteudoMarkdown ? model.Conteudo : model.Conteudo.Trim(),
                ConteudoMarkdown = model.ConteudoMarkdown, CriadoEm = DateTime.UtcNow });
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = Localizer["PostCreateSuccessNotice"].Value;
            return RedirectToAction("View", new { id });
        }
        catch (Exception ex) when (ex is NpgsqlException or DbUpdateException)
        {
            _logger.LogError(ex, "Falha ao criar publicação na comunidade {ComunidadeId}.", id);
            model.ComunidadeId = id;
            TempData["Erro"] = Localizer["PostCreateFailureNotice"].Value;
            return View(model);
        }
    }

    private async Task<int?> CurrentUserId()
    {
        if (User.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(User.Identity.Name)) return null;
        return await _db.Usuarios.Where(u => u.NomeUsuario == User.Identity.Name).Select(u => (int?)u.Id).SingleOrDefaultAsync();
    }

    private Task<bool> NameExists(string nome, int id = 0) =>
        _db.Comunidades.AnyAsync(c => c.Nome == nome && c.Id != id);

    private void DatabaseError(Exception ex)
    {
        _logger.LogError(ex, "Falha ao acessar comunidades.");
        ModelState.AddModelError("", Localizer["CommunityIndexUnavailableNotice"].Value);
    }
}
