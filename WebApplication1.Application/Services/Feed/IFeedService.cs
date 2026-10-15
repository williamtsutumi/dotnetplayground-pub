using WebApplication1.Application.Records;
using WebApplication1.Domain.Feed;

namespace WebApplication1.Application.Services.Feed;

public interface IFeedService
{
    ResultadoService<FeedPaginaRequest> Buscar(int usuarioId, int pagina, string ordem, TipoFeed tipoFeed = TipoFeed.HomePage, int idAutorPost = 0);
    ResultadoService<FeedPaginaRequest> BuscarSalvos(int usuarioId, int pagina);
}
