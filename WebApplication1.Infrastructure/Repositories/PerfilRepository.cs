using WebApplication1.Application.Repositories;
using WebApplication1.Domain.Post;
using WebApplication1.Domain.Usuario;

namespace WebApplication1.Infrastructure.Repositories;

public class PerfilRepository : IPerfilRepository
{
    private readonly RedditDbContext _context;

    public PerfilRepository(RedditDbContext context)
    {
        _context = context;
    }

    public int Gravar(Perfil perfil)
    {
        if (perfil.Id == 0)
        {
            _context.Perfis.Add(perfil);
        }
        else
        {
            _context.Perfis.Update(perfil);
        }
        return perfil.Id;
    }
}
