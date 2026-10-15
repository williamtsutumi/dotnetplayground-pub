using WebApplication1.Application.Records;
using WebApplication1.Domain.Enums;
using WebApplication1.Domain.Feed;

namespace WebApplication1.Application.Repositories;

public interface IFeedRepository
{
    List<FeedPostRequest> Buscar(int usuarioId, int skip, int take, OrdemFeed ordem, TipoFeed tipoFeed, int idAutorPost = 0);
    List<FeedPostRequest> BuscarSalvos(int usuarioId, int skip, int take);
}
