using WebApplication1.Domain.Usuario;

namespace WebApplication1.Application.Repositories;

public interface IPerfilRepository
{
    int Gravar(Perfil perfil);
}
