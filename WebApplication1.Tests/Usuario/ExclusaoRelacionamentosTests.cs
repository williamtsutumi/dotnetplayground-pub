using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure;
using WebApplication1.Models;

namespace WebApplication1.Tests.Usuario;

public class ExclusaoRelacionamentosTests
{
    private static RedditDbContext Context() => new(new DbContextOptionsBuilder<RedditDbContext>()
        .UseNpgsql("Host=localhost;Database=MappingTest;Username=postgres").Options);

    [Theory]
    [InlineData(typeof(Domain.Post.Post), DeleteBehavior.SetNull)]
    [InlineData(typeof(Domain.Comentario.Comentario), DeleteBehavior.SetNull)]
    [InlineData(typeof(Domain.Comunidade.Comunidade), DeleteBehavior.SetNull)]
    [InlineData(typeof(Domain.ComunidadeUsuario.ComunidadeUsuario), DeleteBehavior.Cascade)]
    [InlineData(typeof(Domain.UsuarioUpvotePost.UsuarioUpvotePost), DeleteBehavior.Cascade)]
    [InlineData(typeof(Domain.UsuarioUpvoteComentario.UsuarioUpvoteComentario), DeleteBehavior.Cascade)]
    public void UserDeletionPreservesContentAndRemovesMembershipsAndVotes(Type dependent, DeleteBehavior behavior)
    {
        using var db = Context();
        var fk = Assert.Single(db.Model.FindEntityType(dependent)!.GetForeignKeys(),
            f => f.PrincipalEntityType.ClrType == typeof(Domain.Usuario.Usuario));
        Assert.Equal(behavior, fk.DeleteBehavior);
        Assert.Equal(behavior == DeleteBehavior.Cascade, fk.IsRequired);
    }

    [Fact]
    public void ModelDoesNotContainSoftDeleteColumn()
    {
        using var db = Context();
        Assert.Null(db.Model.FindEntityType(typeof(Domain.Usuario.Usuario))!.FindProperty("Excluido"));
    }

    [Fact]
    public void PostDetailsQueryRetainsPostsWithoutAnAuthor()
    {
        using var db = Context();
        var sql = FeedQueries.Posts(db).ToQueryString();
        Assert.Contains("LEFT JOIN \"Usuario\"", sql);
    }
}
