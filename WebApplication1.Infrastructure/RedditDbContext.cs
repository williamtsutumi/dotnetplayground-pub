using Microsoft.EntityFrameworkCore;
using WebApplication1.Domain.Comentario;
using WebApplication1.Domain.AppGlobalSettings;
using WebApplication1.Domain.ComunidadeUsuario;
using WebApplication1.Domain.Post;
using WebApplication1.Domain.Usuario;
using WebApplication1.Domain.UsuarioUpvoteComentario;
using WebApplication1.Domain.UsuarioUpvotePost;
using WebApplication1.Domain.UsuarioBookmarkPost;

namespace WebApplication1.Infrastructure;

public class RedditDbContext : DbContext
{
    public RedditDbContext(DbContextOptions<RedditDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RedditDbContext).Assembly);
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Domain.Comunidade.Comunidade> Comunidades => Set<Domain.Comunidade.Comunidade>();
    public DbSet<ComunidadeUsuario> ComunidadeUsuarios => Set<ComunidadeUsuario>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<UsuarioUpvotePost> UsuarioUpvotePosts => Set<UsuarioUpvotePost>();
    public DbSet<UsuarioBookmarkPost> UsuarioBookmarkPosts => Set<UsuarioBookmarkPost>();
    public DbSet<UsuarioUpvoteComentario> UsuarioUpvoteComentarios => Set<UsuarioUpvoteComentario>();
    public DbSet<AppGlobalSetting> AppGlobalSettings => Set<AppGlobalSetting>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
}
