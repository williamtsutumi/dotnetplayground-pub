using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.Records;
using WebApplication1.Application.Repositories;
using WebApplication1.Domain.Enums;
using WebApplication1.Domain.Feed;
using WebApplication1.Domain.UsuarioUpvotePost;

namespace WebApplication1.Infrastructure.Repositories;

public class FeedRepository(RedditDbContext db) : IFeedRepository
{
    private IQueryable<FeedPostRequest> ProjetarPosts(int usuarioId) =>
        from p in db.Posts.AsNoTracking()
        join comunidade in db.Comunidades on p.ComunidadeId equals comunidade.Id into comunidades
        from c in comunidades.DefaultIfEmpty()
        join usuario in db.Usuarios on p.IdAutorUsuario equals usuario.Id into autores
        from u in autores.DefaultIfEmpty()
        select new FeedPostRequest
        {
            Id = p.Id,
            ComunidadeId = p.ComunidadeId,
            Comunidade = c == null ? string.Empty : c.Nome,
            TotalComentarios = db.Comentarios.Count(comentario => comentario.IdPost == p.Id),
            TotalUpvotes = db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == TipoVoto.UpVote),
            Upvotes = db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == TipoVoto.UpVote)
                - db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == TipoVoto.DownVote),
            MeuUpvote = db.UsuarioUpvotePosts.Any(v => v.IdPost == p.Id && v.IdUsuario == usuarioId && v.Tipo == TipoVoto.UpVote),
            MeuDownvote = db.UsuarioUpvotePosts.Any(v => v.IdPost == p.Id && v.IdUsuario == usuarioId && v.Tipo == TipoVoto.DownVote),
            Salvo = db.UsuarioBookmarkPosts.Any(b => b.IdUsuario == usuarioId && b.IdPost == p.Id),
            AutorId = u == null ? 0 : u.Id,
            Autor = u == null ? string.Empty : u.NomeUsuario,
            Titulo = p.Titulo,
            Conteudo = p.Conteudo,
            ConteudoMarkdown = p.ConteudoMarkdown,
            CriadoEm = p.CriadoEm
        };

    public List<FeedPostRequest> Buscar(int usuarioId, int skip, int take, OrdemFeed ordem, TipoFeed tipoFeed, int idAutorPost = 0)
    {
        try
        {
            var query = (
                from p in db.Posts.AsNoTracking()
                join comunidade in db.Comunidades on p.ComunidadeId equals comunidade.Id into comunidades
                from c in comunidades.DefaultIfEmpty()
                join usuario in db.Usuarios on p.IdAutorUsuario equals usuario.Id into autores
                from u in autores.DefaultIfEmpty()
                where (
                    (ordem != OrdemFeed.MinhasComunidades || (c != null && (c.IdCriadorUsuario == usuarioId || db.ComunidadeUsuarios.Any(m => m.IdComunidade == c.Id && m.IdUsuario == usuarioId))))
                    && (
                        (tipoFeed == TipoFeed.PerfilFeedPessoal && p.ComunidadeId == null && p.IdAutorUsuario == idAutorPost)
                        || (tipoFeed == TipoFeed.PerfilPosts && p.IdAutorUsuario == idAutorPost)
                        || (tipoFeed == TipoFeed.HomePage && p.ComunidadeId != null)
                    )
                )
                select new FeedPostRequest
                {
                    Id = p.Id,
                    ComunidadeId = p.ComunidadeId,
                    Comunidade = c == null ? string.Empty : c.Nome,
                    TotalComentarios = db.Comentarios.Count(c => c.IdPost == p.Id),
                    TotalUpvotes = db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == TipoVoto.UpVote),
                    Upvotes = db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == TipoVoto.UpVote)
                        - db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == TipoVoto.DownVote),
                    MeuUpvote = db.UsuarioUpvotePosts.Any(v => v.IdPost == p.Id && v.IdUsuario == usuarioId && v.Tipo == TipoVoto.UpVote),
                    MeuDownvote = db.UsuarioUpvotePosts.Any(v => v.IdPost == p.Id && v.IdUsuario == usuarioId && v.Tipo == TipoVoto.DownVote),
                    Salvo = db.UsuarioBookmarkPosts.Any(b => b.IdUsuario == usuarioId && b.IdPost == p.Id),
                    AutorId = u == null ? 0 : u.Id,
                    Autor = u == null ? string.Empty : u.NomeUsuario,
                    Titulo = p.Titulo,
                    Conteudo = p.Conteudo,
                    ConteudoMarkdown = p.ConteudoMarkdown,
                    CriadoEm = p.CriadoEm
                }
            );

            var descending = ordem.IsDescendingOrder();

            if (ordem == OrdemFeed.MaisPopulares)
            {
                query = query
                        .OrderByDescending(q => q.TotalUpvotes)
                        .ThenBy(q => q.CriadoEm);
            }
            else if (descending)
            {
                query = query
                        .OrderByDescending(p => p.CriadoEm);
            }
            else
            {
                query = query
                        .OrderBy(p => p.CriadoEm);
            }


            return query
                    .Skip(skip)
                    .Take(take)
                    .ToList();
        }
        catch (Exception ex)
        {
            return new List<FeedPostRequest>();
        }
    }

    public List<FeedPostRequest> BuscarSalvos(int usuarioId, int skip, int take)
    {
        return (from bookmark in db.UsuarioBookmarkPosts.AsNoTracking()
                join post in ProjetarPosts(usuarioId) on bookmark.IdPost equals post.Id
                where bookmark.IdUsuario == usuarioId
                orderby post.CriadoEm descending
                select post)
            .Skip(skip)
            .Take(take)
            .ToList();
    }
}
