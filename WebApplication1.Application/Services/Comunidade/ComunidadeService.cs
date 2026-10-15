using WebApplication1.Application.Records;
using WebApplication1.Application.Repositories;

namespace WebApplication1.Application.Services.Comunidade;


public class ComunidadeService : IComunidadeService
{
    private readonly IComunidadeRepository _comunidadeRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public ComunidadeService(IComunidadeRepository comunidadeRepository, IUsuarioRepository usuarioRepository)
    {
        _comunidadeRepository = comunidadeRepository;
        _usuarioRepository = usuarioRepository;
    }


    public ResultadoService<int> Salvar(CriarComunidadeResponse command)
    {
        if (command.UsuarioId is null || command.UsuarioId == 0)
            return ResultadoService<int>.Falha("FalhaUsuarioNaoReconhecido");

        if (!_usuarioRepository.IsValidUsuarioVerificado(command.UsuarioNome ?? ""))
            return ResultadoService<int>.Falha("FalhaUsuarioConvidado");

        if (!_comunidadeRepository.IsValidNomeSemDuplicidade(command.Nome, command.Id))
            return ResultadoService<int>.Falha("FalhaJaExisteNomeComunidade");

        var comunidadeNova = new Domain.Comunidade.Comunidade
        {
            Id = command.Id,
            Nome = command.Nome,
            Descricao = command.Descricao?.Trim(),
            IdCriadorUsuario = command.UsuarioId.Value,
            CriadoEm = DateTime.UtcNow
        };

        if (command.Id > 0)
        {
            var comunidadeSalva = _comunidadeRepository.BuscarPorId(command.Id);
            if (comunidadeSalva is null)
                return ResultadoService<int>.Falha("FalhaEditComunidadeNaoEncontrada");

            if (comunidadeSalva.IdCriadorUsuario != command.UsuarioId)
                return ResultadoService<int>.Falha("FalhaApenasCriadorPodeEditar");

            comunidadeNova.Nome = comunidadeSalva.Nome;
        }

        _comunidadeRepository.Salvar(comunidadeNova);
        _comunidadeRepository.SaveChanges();

        return ResultadoService<int>.Ok(comunidadeNova.Id);
    }

    public void Deletar(int id)
    {
        throw new NotImplementedException();
    }

    public Domain.Comunidade.Comunidade? BuscarPorId(int id)
    {
        return _comunidadeRepository.BuscarPorId(id);
    }

}
