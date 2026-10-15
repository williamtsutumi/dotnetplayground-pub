using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure;
using WebApplication1.Models;
using System.Security.Claims;
using Npgsql;
using WebApplication1.Domain.UsuarioUpvotePost;
using WebApplication1.Domain.UsuarioBookmarkPost;
using Microsoft.Extensions.Localization;
using WebApplication1.Models.Comunidade;
using WebApplication1.Domain.Post;
using WebApplication1.Application.Services.Post;
using WebApplication1.Application.Services.Feed;
using AutoMapper;
namespace WebApplication1.Controllers;

public class PostController : Controller
{
    private IStringLocalizer<SharedResource> Localizer => HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
    private readonly RedditDbContext _db;

    private readonly IPostService _postService;
    private readonly IFeedService _feedService;
    private readonly IMapper _mapper;

    public PostController(RedditDbContext db, IPostService postService, IFeedService feedService, IMapper mapper)
    {
        _db = db;
        _postService = postService;
        _feedService = feedService;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Bookmarked(int pagina = 1)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)) return Challenge();

        ViewData["PostContentReduced"] = true;
        var resultado = _feedService.BuscarSalvos(usuarioId, pagina);
        var model = _mapper.Map<FeedModel>(resultado.Valor);
        model.Indisponivel = !resultado.Sucesso;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Bookmark(int id, string? returnUrl)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)) return Challenge();
        if (!await _db.Usuarios.AnyAsync(u => u.Id == usuarioId, HttpContext.RequestAborted)) return Challenge();
        if (!await _db.Posts.AnyAsync(p => p.Id == id, HttpContext.RequestAborted)) return NotFound();

        var bookmark = await _db.UsuarioBookmarkPosts.SingleOrDefaultAsync(
            b => b.IdUsuario == usuarioId && b.IdPost == id,
            HttpContext.RequestAborted);

        var salvando = bookmark is null;
        if (salvando)
            _db.UsuarioBookmarkPosts.Add(new UsuarioBookmarkPost { IdUsuario = usuarioId, IdPost = id });
        else
            _db.UsuarioBookmarkPosts.Remove(bookmark!);

        try
        {
            await _db.SaveChangesAsync(HttpContext.RequestAborted);
            TempData["Sucesso"] = Localizer[salvando ? "BookmarkSaved" : "BookmarkRemoved"].Value;
        }
        catch (Exception ex) when (ex is NpgsqlException or DbUpdateException)
        {
            TempData["Erro"] = Localizer["BookmarkError"].Value;
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("View", new { id });
    }

    [HttpGet]
    public IActionResult Criar(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUser)) return Challenge();

        var model = new CriarPostModel
        {
            ComunidadesPermitidas = ComunidadesPermitidas(idUser).OrderBy(c => c.Nome).Select(c => c.Nome).ToList(),
        };

        var post = _db.Posts.Include(p => p.Comunidade).FirstOrDefault(q => q.Id == id);
        if (post != null)
        {
            model.PostId = post.Id;
            model.Titulo = post.Titulo;
            model.Conteudo = post.Conteudo;
            model.ConteudoMarkdown = post.ConteudoMarkdown;
            model.ComunidadeId = post.ComunidadeId;
            model.ComunidadeNome = post.Comunidade?.Nome;
            model.ComunidadeDestino = post.Comunidade?.Nome;
        }

        return View(model);
    }

    private IQueryable<Domain.Comunidade.Comunidade> ComunidadesPermitidas(int id) =>
        _db.Comunidades.AsNoTracking().Where(c => c.IdCriadorUsuario == id ||
            _db.ComunidadeUsuarios.Any(m => m.IdComunidade == c.Id && m.IdUsuario == id));

    [HttpGet]
    public async Task<IActionResult> ValidarComunidade(string? nome)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Unauthorized();
        var valido = string.IsNullOrWhiteSpace(nome) || await ComunidadesPermitidas(id).AnyAsync(c => c.Nome == nome.Trim());
        return Json(new { valido, message = valido ? "" : Localizer["PostCommunityPermissionError"].Value });
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarPostModel model)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)) return Challenge();

        try
        {
            if (!await _db.Usuarios.AnyAsync(u => u.Id == usuarioId))
                return Challenge();

            model.ComunidadesPermitidas = await ComunidadesPermitidas(usuarioId).OrderBy(c => c.Nome).Select(c => c.Nome).ToListAsync();
            int? comunidadeId = null;
            if (!string.IsNullOrWhiteSpace(model.ComunidadeDestino))
            {
                comunidadeId = await ComunidadesPermitidas(usuarioId).Where(c => c.Nome == model.ComunidadeDestino.Trim()).Select(c => (int?)c.Id).SingleOrDefaultAsync();
                if (comunidadeId is null) ModelState.AddModelError(nameof(model.ComunidadeDestino), Localizer["PostCommunityPermissionError"].Value);
            }

            if (!ModelState.IsValid)
            {
                TempData["Erro"] = Localizer["PostCreateFieldsError"].Value;
                return View(model);
            }

            var post = new Post
            {
                Id = model.PostId ?? 0,
                ComunidadeId = comunidadeId,
                IdAutorUsuario = usuarioId,
                Titulo = model.Titulo.Trim(),
                Conteudo = model.ConteudoMarkdown ? model.Conteudo : model.Conteudo.Trim(),
                ConteudoMarkdown = model.ConteudoMarkdown,
            };
            if (post.Id != 0)
            {
                var postOld = _db.Posts.AsNoTracking().FirstOrDefault(q => q.Id == model.PostId);
                if (postOld is null)
                {
                    TempData["Erro"] = Localizer["PostCreateFieldsError"].Value;
                    return View(model);
                }
                post.CriadoEm = postOld.CriadoEm;
                post.EditadoEm = DateTime.UtcNow;
                _db.Posts.Update(post);

                TempData["Sucesso"] = Localizer["AlteracoesSalvasSuccess"].Value;
            }
            else
            {
                post.CriadoEm = DateTime.UtcNow;
                _db.Posts.Add(post);

                TempData["Sucesso"] = Localizer["PostCreateSuccessNotice"].Value;
            }
            await _db.SaveChangesAsync();

            

            return RedirectToAction("View", new { id = post.Id, voltarHome = true });
        }
        catch (Exception ex) when (ex is NpgsqlException or DbUpdateException)
        {
            TempData["Erro"] = Localizer["PostCreateFailureNotice"].Value;
            return View(model);
        }
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var resposta = _postService.Delete(id);

        if (!resposta.Sucesso)
        {
            TempData["Erro"] = Localizer[resposta.CodigoErro!].Value;
            return RedirectToAction("View", "Post", new { id });
        }

        TempData["Sucesso"] = Localizer["ExclusaoSucesso"].Value;
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous, HttpGet]
    public Task<IActionResult> View(int id, bool voltarHome = false)
    {
        ViewData["VoltarHome"] = voltarHome;
        return CarregarDetalhes(id);
    }

    private async Task<IActionResult> CarregarDetalhes(int id, string novoComentario = "", int? respondendoId = null)
    {
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var leitorId);
        var post = await FeedQueries.Posts(_db, leitorId: leitorId).SingleOrDefaultAsync(p => p.Id == id, HttpContext.RequestAborted);
        if (post is null) return NotFound();

        post.CurrentUserIsAutor = leitorId > 0 && leitorId == post.AutorId;

        var consulta = (
            from comentario in _db.Comentarios.AsNoTracking()
            join usuario in _db.Usuarios on comentario.IdAutorUsuario equals usuario.Id into autores
            from autor in autores.DefaultIfEmpty()
            select new { Comentario = comentario, Autor = autor == null ? string.Empty : autor.NomeUsuario });
        var encontrados = await consulta.Where(c => c.Comentario.IdPost == id).ToListAsync(HttpContext.RequestAborted);
        var vistos = encontrados.Select(c => c.Comentario.Id).ToHashSet();
        var pendentes = vistos.ToList();
        while (pendentes.Count > 0)
        {
            var filhos = await consulta.Where(c => c.Comentario.IdComentario.HasValue && pendentes.Contains(c.Comentario.IdComentario.Value)
                && (c.Comentario.IdPost == null || c.Comentario.IdPost == id)).ToListAsync(HttpContext.RequestAborted);
            var novos = filhos.Where(c => vistos.Add(c.Comentario.Id)).ToList();
            encontrados.AddRange(novos);
            pendentes = novos.Select(c => c.Comentario.Id).ToList();
        }
        var todos = encontrados.Select(c => new ComentarioModel
            {
                Id = c.Comentario.Id, IdComentario = c.Comentario.IdComentario,
                AutorId = c.Comentario.IdAutorUsuario ?? 0, Autor = c.Autor, Texto = c.Comentario.Texto,
                CriadoEm = c.Comentario.CriadoEm
            }).OrderBy(c => c.Id).ToList();
        var porPai = todos.Where(c => c.IdComentario.HasValue).ToLookup(c => c.IdComentario!.Value);
        var pilha = new Stack<(ComentarioModel Comentario, int Nivel)>(todos.Where(c => c.IdComentario == null).Reverse().Select(c => (c, 0)));
        var comentarios = new List<ComentarioModel>();
        var exibidos = new HashSet<int>();
        while (pilha.TryPop(out var item))
        {
            if (!exibidos.Add(item.Comentario.Id)) continue;
            item.Comentario.Profundidade = item.Nivel;
            comentarios.Add(item.Comentario);
            foreach (var filho in porPai[item.Comentario.Id].Reverse()) pilha.Push((filho, item.Nivel + 1));
        }

        var erroResposta = "";
        if (respondendoId.HasValue)
        {
            erroResposta = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            ModelState.Clear();
        }

        return View("Detalhes", new PostDetalhesModel
        {
            Post = post,
            Comentarios = comentarios,
            NovoComentario = respondendoId.HasValue ? "" : novoComentario,
            RespondendoId = respondendoId,
            TextoResposta = novoComentario,
            ErroResposta = erroResposta
        });
    }

    [HttpPost]
    public async Task<IActionResult> Comentar(int id, string? novoComentario, int? idComentario = null)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)) return Challenge();
        if (!await _db.Usuarios.AnyAsync(u => u.Id == usuarioId, HttpContext.RequestAborted)) return Challenge();
        if (!await _db.Posts.AnyAsync(p => p.Id == id, HttpContext.RequestAborted)) return NotFound();
        if (idComentario.HasValue)
        {
            var atual = idComentario;
            var visitados = new HashSet<int>();
            while (atual.HasValue)
            {
                if (!visitados.Add(atual.Value)) return BadRequest();
                var pai = await _db.Comentarios.AsNoTracking().SingleOrDefaultAsync(c => c.Id == atual.Value, HttpContext.RequestAborted);
                if (pai is null || (pai.IdPost.HasValue && pai.IdPost != id)) return NotFound();
                if (pai.IdComentario == null && pai.IdPost != id) return NotFound();
                atual = pai.IdComentario;
            }
        }

        if (string.IsNullOrWhiteSpace(novoComentario))
            ModelState.AddModelError(nameof(PostDetalhesModel.NovoComentario), Localizer["PostCommentRequiredError"].Value);
        else if (novoComentario.Length > 3000)
            ModelState.AddModelError(nameof(PostDetalhesModel.NovoComentario), Localizer["PostCommentLengthError"].Value);

        if (!ModelState.IsValid) return await CarregarDetalhes(id, novoComentario ?? "", idComentario);

        var comentario = new Domain.Comentario.Comentario
        {
            IdPost = id,
            IdComentario = idComentario,
            IdAutorUsuario = usuarioId,
            Texto = novoComentario!.Trim(),
            CriadoEm = DateTime.UtcNow
        };
        _db.Comentarios.Add(comentario);
        try
        {
            await _db.SaveChangesAsync(HttpContext.RequestAborted);
        }
        catch (Exception ex) when (ex is NpgsqlException or DbUpdateException)
        {
            _db.Entry(comentario).State = EntityState.Detached;
            ModelState.AddModelError(nameof(PostDetalhesModel.NovoComentario), Localizer["PostCommentFailureNotice"].Value);
            return await CarregarDetalhes(id, novoComentario!, idComentario);
        }

        TempData["Sucesso"] = Localizer["PostCommentSuccessNotice"].Value;
        return RedirectToAction("View", "Post", new { id }, $"comentario-{comentario.Id}");
    }

    [HttpPost]
    public async Task<IActionResult> Votar(int id, TipoVoto? tipo, bool remover = false)
    {
        var localizer = HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        if (!ModelState.IsValid || (!remover && tipo is not (TipoVoto.UpVote or TipoVoto.DownVote)))
            return BadRequest(new { message = localizer["VoteInvalid"].Value });

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized(new { message = localizer["VoteLoginAgain"].Value });

        try
        {
            if (!await _db.Usuarios.AnyAsync(u => u.Id == usuarioId))
                return Unauthorized(new { message = localizer["VoteLoginAgain"].Value });

            if (!await _db.Posts.AnyAsync(p => p.Id == id))
                return NotFound(new { message = localizer["PostNotFound"].Value });

            if (!remover)
            {
                var atualizados = await _db.UsuarioUpvotePosts.Where(v => v.IdPost == id && v.IdUsuario == usuarioId)
                    .ExecuteUpdateAsync(s => s.SetProperty(v => v.Tipo, tipo!.Value));
                if (atualizados == 0)
                {
                    var voto = new UsuarioUpvotePost { IdUsuario = usuarioId, IdPost = id, Tipo = tipo!.Value };
                    _db.UsuarioUpvotePosts.Add(voto);
                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                    {
                        _db.Entry(voto).State = EntityState.Detached;
                        await _db.UsuarioUpvotePosts.Where(v => v.IdPost == id && v.IdUsuario == usuarioId)
                            .ExecuteUpdateAsync(s => s.SetProperty(v => v.Tipo, tipo!.Value));
                    }
                }
            }
            else
                await _db.UsuarioUpvotePosts.Where(v => v.IdPost == id && v.IdUsuario == usuarioId).ExecuteDeleteAsync();

            var meuTipo = await _db.UsuarioUpvotePosts.Where(v => v.IdPost == id && v.IdUsuario == usuarioId)
                .Select(v => (TipoVoto?)v.Tipo).SingleOrDefaultAsync();

            var saldo = await _db.UsuarioUpvotePosts.Where(v => v.IdPost == id)
                .SumAsync(v => v.Tipo == TipoVoto.UpVote ? 1 : v.Tipo == TipoVoto.DownVote ? -1 : 0);

            return Json(new {
                total = saldo,
                voted = meuTipo == TipoVoto.UpVote,
                downvoted = meuTipo == TipoVoto.DownVote,
                message = remover ? localizer["VoteRemoved"].Value : tipo == TipoVoto.UpVote ? localizer["VoteAdded"].Value : localizer["DownvoteAdded"].Value
            });
        }
        catch (Exception ex) when (ex is NpgsqlException or DbUpdateException)
        {
            return StatusCode(500, new { message = localizer["VoteServerError"].Value });
        }
    }
}
