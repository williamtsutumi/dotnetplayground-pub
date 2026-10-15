namespace WebApplication1.Application.Repositories;

public interface IUsuarioRepository
{
    void Deletar(int id);
    bool AtualizarDescricao(int id, string? descricao);
    public void Salvar(Domain.Usuario.Usuario usuario);
    Task<Domain.Usuario.Usuario?> BuscarPorNomeUsuarioOuEmailAsync(string identificador);
    public bool IsValidUsuarioVerificado(string usuarioNome);
    Domain.Usuario.Usuario? BuscarPorId(int id);
}
