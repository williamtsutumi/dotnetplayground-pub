using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication1.Application.Records;
using WebApplication1.Application.Repositories;
using WebApplication1.Application.Services.Feed;
using WebApplication1.Domain.Enums;
using WebApplication1.Domain.Feed;
using WebApplication1.Infrastructure.Repositories;

namespace WebApplication1.Tests.Feed;

public class FeedServiceTests
{
    [Fact]
    public void BuscarFeedPaginaDeveRetornarOrdemCorreta()
    {
        // Setup
        var repository = new Mock<IFeedRepository>();
        repository
            .Setup(r => r.Buscar(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrdemFeed>(), It.IsAny<TipoFeed>(), It.IsAny<int>()))
            .Returns((List<FeedPostRequest>?) null);

        // Execute
        var result = new FeedService(repository.Object).Buscar(1, 2, It.IsAny<string>());

        // Assert
        Assert.False(result.Sucesso);

        repository.Verify(
            r => r.Buscar(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrdemFeed>(), It.IsAny<TipoFeed>(), It.IsAny<int>()),
            Times.Once()
        );
    }

    [Fact]
    public void HomeFeedNaoDeveIncluirPostsSemComunidade()
    {
        // Setup
        using var context = TestDbContextFactory.CriarRedditDbContext();
        
        var repository = new FeedRepository(context);
        var resultado = repository.Buscar(usuarioId: 1, skip: 0, take: 20, ordem: OrdemFeed.MaisRecentes, tipoFeed: TipoFeed.HomePage, idAutorPost: 0);

        Assert.NotEmpty(resultado);
        Assert.All(resultado, r => Assert.NotNull(r.ComunidadeId));
    }

    [Fact]
    public void PerfilFeedPessoalDeveTerPostsApenasSemComunidade()
    {
        using var context = TestDbContextFactory.CriarRedditDbContext();

        var repository = new FeedRepository(context);
        var resultado = repository.Buscar(usuarioId: 1, skip: 0, take: 20, ordem: OrdemFeed.MaisRecentes, tipoFeed: TipoFeed.PerfilFeedPessoal, idAutorPost: 4);

        Assert.NotEmpty(resultado);
        Assert.All(resultado, r => Assert.Null(r.ComunidadeId));
        Assert.All(resultado, r => Assert.Equal(4, r.AutorId));
    }

    [Fact]
    public void PerfilFeedPostsDeveTerPostsComOuSenComunidade()
    {
        using var context = TestDbContextFactory.CriarRedditDbContext();

        var repository = new FeedRepository(context);
        var resultado = repository.Buscar(usuarioId: 1, skip: 0, take: 20, ordem: OrdemFeed.MaisRecentes, tipoFeed: TipoFeed.PerfilPosts, idAutorPost: 4);

        Assert.NotEmpty(resultado);
        Assert.Contains(resultado, r => r.ComunidadeId == null);
        Assert.Contains(resultado, r => r.ComunidadeId != null);
        Assert.All(resultado, r => Assert.Equal(4, r.AutorId));
    }

    [Theory]
    [InlineData(OrdemFeed.MaisAntigos, TipoFeed.HomePage)]
    [InlineData(OrdemFeed.MaisAntigos, TipoFeed.PerfilPosts)]
    [InlineData(OrdemFeed.MaisAntigos, TipoFeed.PerfilFeedPessoal)]
    public void DeveOrdenarPorMaisAntigos(OrdemFeed ordem, TipoFeed tipo)
    {
        using var context = TestDbContextFactory.CriarRedditDbContext();

        var repository = new FeedRepository(context);
        var resultado = repository.Buscar(usuarioId: 1, skip: 0, take: 20, ordem: ordem, tipoFeed: tipo, idAutorPost: 4);

        Assert.NotEmpty(resultado);
        Assert.Equal(resultado.OrderBy(r => r.CriadoEm), resultado);
    }

    [Theory]
    [InlineData(OrdemFeed.MaisRecentes, TipoFeed.HomePage)]
    [InlineData(OrdemFeed.MaisRecentes, TipoFeed.PerfilPosts)]
    [InlineData(OrdemFeed.MaisRecentes, TipoFeed.PerfilFeedPessoal)]
    public void DeveOrdenarPorMaisRecentes(OrdemFeed ordem, TipoFeed tipo)
    {
        using var context = TestDbContextFactory.CriarRedditDbContext();

        var repository = new FeedRepository(context);
        var resultado = repository.Buscar(usuarioId: 1, skip: 0, take: 20, ordem: ordem, tipoFeed: tipo, idAutorPost: 4);

        Assert.NotEmpty(resultado);
        Assert.Equal(resultado.OrderByDescending(r => r.CriadoEm), resultado);
    }

    [Theory]
    [InlineData(OrdemFeed.MaisPopulares, TipoFeed.HomePage)]
    [InlineData(OrdemFeed.MaisPopulares, TipoFeed.PerfilPosts)]
    [InlineData(OrdemFeed.MaisPopulares, TipoFeed.PerfilFeedPessoal)]
    public void DeveOrdenarPorMaisPopulares(OrdemFeed ordem, TipoFeed tipo)
    {
        using var context = TestDbContextFactory.CriarRedditDbContext();

        var repository = new FeedRepository(context);
        var resultado = repository.Buscar(usuarioId: 1, skip: 0, take: 20, ordem: ordem, tipoFeed: tipo, idAutorPost: 4);

        Assert.NotEmpty(resultado);
        Assert.Equal(resultado.OrderByDescending(r => r.Upvotes), resultado);
    }

    [Theory]
    [InlineData(1, 22, true)]
    [InlineData(1, 21, true)]
    [InlineData(1, 20, false)]
    [InlineData(1, 19, false)]
    [InlineData(2, 22, true)]
    [InlineData(2, 21, true)]
    [InlineData(2, 20, false)]
    [InlineData(2, 19, false)]
    public void TemProximaDeveSerTrueQuandoNaoEhUltimaPagina(int pagina, int repoCount, bool esperado)
    {
        // Setup
        var repository = new Mock<IFeedRepository>();
        repository
            .Setup(r => r.Buscar(It.IsAny<int>(), 20 * (pagina-1), 21, It.IsAny<OrdemFeed>(), It.IsAny<TipoFeed>(), It.IsAny<int>()))
            .Returns(Enumerable.Repeat(new FeedPostRequest(), repoCount).ToList());

        // Execute
        var result = new FeedService(repository.Object).Buscar(It.IsAny<int>(), pagina, It.IsAny<string>());

        // Assert
        Assert.True(result.Sucesso);
        Assert.Equal(result.Valor.TemProxima, esperado);
        Assert.Equal(result.Valor.Posts.Count(), Math.Min(20, repoCount));
    }
}
