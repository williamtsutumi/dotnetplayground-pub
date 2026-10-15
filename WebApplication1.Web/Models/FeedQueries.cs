using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure;
namespace WebApplication1.Models;
public static class FeedQueries
{
    public static IQueryable<FeedPostModel> Posts(RedditDbContext db, int? usuarioId = null, int? leitorId = null) =>
        from p in db.Posts.AsNoTracking()
        join comunidade in db.Comunidades on p.ComunidadeId equals comunidade.Id into comunidades
        from c in comunidades.DefaultIfEmpty()
        join usuario in db.Usuarios on p.IdAutorUsuario equals usuario.Id into autores
        from u in autores.DefaultIfEmpty()
        where usuarioId == null || (p.ComunidadeId == null && p.IdAutorUsuario == usuarioId)
            || (c != null && (c.IdCriadorUsuario == usuarioId || db.ComunidadeUsuarios.Any(m => m.IdComunidade == c.Id && m.IdUsuario == usuarioId)))
        select new FeedPostModel
        {
            Id = p.Id,
            ComunidadeId = p.ComunidadeId,
            Comunidade = c == null ? string.Empty : c.Nome,
            TotalComentarios = db.Comentarios.Count(c => c.IdPost == p.Id),
            TotalUpvotes = db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == Domain.UsuarioUpvotePost.TipoVoto.UpVote),
            Upvotes = db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == Domain.UsuarioUpvotePost.TipoVoto.UpVote)
                - db.UsuarioUpvotePosts.Count(v => v.IdPost == p.Id && v.Tipo == Domain.UsuarioUpvotePost.TipoVoto.DownVote),
            MeuUpvote = db.UsuarioUpvotePosts.Any(v => v.IdPost == p.Id && v.IdUsuario == (leitorId ?? usuarioId) && v.Tipo == Domain.UsuarioUpvotePost.TipoVoto.UpVote),
            MeuDownvote = db.UsuarioUpvotePosts.Any(v => v.IdPost == p.Id && v.IdUsuario == (leitorId ?? usuarioId) && v.Tipo == Domain.UsuarioUpvotePost.TipoVoto.DownVote),
            Salvo = db.UsuarioBookmarkPosts.Any(b => b.IdPost == p.Id && b.IdUsuario == (leitorId ?? usuarioId)),
            AutorId = u == null ? 0 : u.Id,
            Autor = u == null ? string.Empty : u.NomeUsuario,
            Titulo = p.Titulo,
            Conteudo = p.Conteudo,
            ConteudoMarkdown = p.ConteudoMarkdown,
            CriadoEm = p.CriadoEm
        };
}
