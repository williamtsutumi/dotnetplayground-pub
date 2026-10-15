using WebApplication1.Application.Records;

namespace WebApplication1.Application.Services.Usuario;

public interface IUsuarioService
{
    ResultadoService<int> AtualizarDescricao(int id, WebApplication1.Application.Commands.Usuario.EditarDescricaoCommand command);
    ResultadoService<int> Salvar(SalvarUsuarioRequest command);
    Task<Domain.Usuario.Usuario?> AutenticarAsync(string usuario, string senha);
    void Deletar(int id);
    Domain.Usuario.Usuario Buscar(int id);
}
