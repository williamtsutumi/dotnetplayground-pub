using WebApplication1.Domain.UsuarioUpvotePost;

namespace WebApplication1.Application.Repositories;

public interface IVoteRepository
{
    UsuarioUpvotePost? BuscarPorPostId(int id);
}
