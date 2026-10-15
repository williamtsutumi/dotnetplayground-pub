using WebApplication1.Domain.Comentario;

namespace WebApplication1.Application.Repositories;

public interface IComentarioRepository
{
    Comentario? BuscarPorPostId(int id);
}
