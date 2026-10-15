using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.Repositories;

namespace WebApplication1.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly RedditDbContext _context;

    public UsuarioRepository(RedditDbContext context)
    {
        _context = context;
    }

    public void Salvar(Domain.Usuario.Usuario usuario)
    {
        if (_context.Usuarios.Any(u => u.NomeUsuario == usuario.NomeUsuario))
            throw new ArgumentException("Este nome de usuário já está cadastrado.");
        if (_context.Usuarios.Any(u => u.Email == usuario.Email))
            throw new ArgumentException("Este e-mail já está cadastrado.");
        _context.Usuarios.Add(usuario);
    }

    public async Task<Domain.Usuario.Usuario?> BuscarPorNomeUsuarioOuEmailAsync(string identificador)
    {
        var encontrados = await _context.Usuarios.AsNoTracking()
            .Include(u => u.Perfis)
            .Where(u => u.NomeUsuario == identificador || u.Email == identificador)
            .Take(2).ToListAsync();
        // Não escolha arbitrariamente uma conta se o username coincidir com o e-mail de outra.
        return encontrados.Count == 1 ? encontrados[0] : null;
    }

    public bool IsValidUsuarioVerificado(string usuarioNome) => true;

    public bool AtualizarDescricao(int id, string? descricao)
    {
        var usuario = _context.Usuarios.Find(id);
        if (usuario is null) return false;
        usuario.Descricao = descricao;
        _context.SaveChanges();
        return true;
    }

    public Domain.Usuario.Usuario? BuscarPorId(int id)
    {
        return _context.Usuarios.FirstOrDefault(q => q.Id == id);
    }

    public void Deletar(int id)
    {
        _context.Usuarios.Where(u => u.Id == id).ExecuteDelete();
    }
}
