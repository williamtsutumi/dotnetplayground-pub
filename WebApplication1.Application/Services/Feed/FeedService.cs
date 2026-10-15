using WebApplication1.Application.Records;
using WebApplication1.Application.Repositories;
using WebApplication1.Domain.Feed;

namespace WebApplication1.Application.Services.Feed;

public class FeedService(IFeedRepository repository) : IFeedService
{
    private const int PAGE_SIZE = 20;

    public ResultadoService<FeedPaginaRequest> Buscar(int usuarioId, int pagina, string ordem, TipoFeed tipoFeed = TipoFeed.HomePage, int idAutorPost = 0)
    {
        var ordemEnum = ordem.ToEnumOrdemFeed();
        var descending = ordemEnum.IsDescendingOrder();

        pagina = Math.Clamp(pagina, 1, 100000);

        var posts = repository.Buscar(
                usuarioId,
                (pagina - 1) * PAGE_SIZE,
                PAGE_SIZE+1,
                ordemEnum,
                tipoFeed,
                idAutorPost);

        if (posts == null)
            return ResultadoService<FeedPaginaRequest>.Falha("FalhaFeedPaginaNenhumPostEncontrado");

        return ResultadoService<FeedPaginaRequest>.Ok(
            new FeedPaginaRequest(pagina, posts.Count > PAGE_SIZE, posts.Take(PAGE_SIZE).ToList())
        );
    }

    public ResultadoService<FeedPaginaRequest> BuscarSalvos(int usuarioId, int pagina)
    {
        pagina = Math.Clamp(pagina, 1, 100000);
        var posts = repository.BuscarSalvos(usuarioId, (pagina - 1) * PAGE_SIZE, PAGE_SIZE + 1);
        if (posts == null)
            return ResultadoService<FeedPaginaRequest>.Falha("FalhaFeedPaginaNenhumPostEncontrado");

        return ResultadoService<FeedPaginaRequest>.Ok(
            new FeedPaginaRequest(pagina, posts.Count > PAGE_SIZE, posts.Take(PAGE_SIZE).ToList()));
    }
}
