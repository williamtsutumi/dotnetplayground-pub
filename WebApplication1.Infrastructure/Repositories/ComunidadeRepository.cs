using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.Repositories;

namespace WebApplication1.Infrastructure.Repositories;

public class ComunidadeRepository : IComunidadeRepository
{
    private readonly RedditDbContext _context;

    public ComunidadeRepository(RedditDbContext context)
    {
        _context = context;
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public void Salvar(Domain.Comunidade.Comunidade comunidade)
    {
        if (comunidade.Id == 0)
        {
            _context.Comunidades.Add(comunidade);
        }
        else
        {
            _context.Comunidades.Update(comunidade);
        }
    }

    public bool IsValidNomeSemDuplicidade(string nomeComunidade, int id = 0)
    {
        return !_context.Comunidades.Any(q => q.Nome == nomeComunidade && (id == 0 || q.Id != id));
    }

    public Domain.Comunidade.Comunidade? BuscarPorId(int id)
    {
        return _context.Comunidades.AsNoTracking().FirstOrDefault(q => q.Id == id);
    }
}
