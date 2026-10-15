using WebApplication1.Application.Records;

namespace WebApplication1.Application.Services.Comunidade;

public interface IComunidadeService
{
    ResultadoService<int> Salvar(CriarComunidadeResponse command);
    void Deletar(int id);
    Domain.Comunidade.Comunidade BuscarPorId(int id);
}
