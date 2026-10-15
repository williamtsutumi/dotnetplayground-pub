using System.Security.Cryptography;
using WebApplication1.Application.Records;
using WebApplication1.Application.Repositories;

namespace WebApplication1.Application.Services.Usuario;


public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPerfilRepository _perfilRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UsuarioService(IUsuarioRepository usuarioRepository, IUnitOfWork unitOfWork, IPerfilRepository perfilRepository)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _perfilRepository = perfilRepository;
    }

    public ResultadoService<int> Salvar(SalvarUsuarioRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Nome?.Any(char.IsWhiteSpace) == true)
            return ResultadoService<int>.Falha("UsernameNoWhitespace");

        if (string.IsNullOrWhiteSpace(request.Nome) || request.Nome.Trim().Length > 50)
            return ResultadoService<int>.Falha("FalhaNomeUsuarioLongo");

        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Trim().Length > 255 ||
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email.Trim()))
            return ResultadoService<int>.Falha("FalhaEmailLongo");

        if (string.IsNullOrWhiteSpace(request.Senha))
            return ResultadoService<int>.Falha("FalhaUsuarioDeveTerSenha");

        if (request.TiposPerfil.Count() == 0)
            return ResultadoService<int>.Falha("FalhaUsuarioDeveTerPerfil");

        //if (request.DataNascimento.Date > DateTime.Today || request.DataNascimento.Date < DateTime.Today.AddYears(-150))
        //    throw new ArgumentException("Informe uma data de nascimento válida.");

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(request.Senha, salt, 600000, HashAlgorithmName.SHA256, 32);

        try
        {
            var resultado = _unitOfWork.ExecuteInTransaction(() =>
            {
                var usuario = new Domain.Usuario.Usuario()
                {
                    Nome = request.Nome.Trim(),
                    NomeUsuario = request.Nome.Trim(),
                    Email = request.Email.Trim(),
                    Senha = $"PBKDF2-SHA256$600000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}",
                    Descricao = request.Descricao
                };
                _usuarioRepository.Salvar(usuario);

                _unitOfWork.SaveChanges();

                foreach (var tipoPerfil in request.TiposPerfil)
                {
                    _perfilRepository.Gravar(new Domain.Usuario.Perfil()
                    {
                        IdUsuario = usuario.Id,
                        Tipo = tipoPerfil
                    });
                }

                _unitOfWork.SaveChanges();

                return usuario.Id;
            });

            return ResultadoService<int>.Ok(resultado);
        }
        catch (Exception ex)
        {
            return ResultadoService<int>.Falha(ex.Message);
        }
    }

    public async Task<Domain.Usuario.Usuario?> AutenticarAsync(string usuario, string senha)
    {
        if (string.IsNullOrWhiteSpace(usuario) || usuario.Trim().Length > 255 ||
            string.IsNullOrEmpty(senha) || senha.Length > 128)
            return null;

        var cadastrado = await _usuarioRepository.BuscarPorNomeUsuarioOuEmailAsync(usuario.Trim());
        var parts = cadastrado?.Senha?.Split('$');
        if (parts is not { Length: 4 } || parts[0] != "PBKDF2-SHA256" || parts[1] != "600000")
            return null;

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            if (salt.Length != 16 || expected.Length != 32)
                return null;
            var actual = Rfc2898DeriveBytes.Pbkdf2(senha, salt, 600000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(actual, expected) ? cadastrado : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public void Deletar(int id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        _usuarioRepository.Deletar(id);
    }

    public ResultadoService<int> AtualizarDescricao(int id, WebApplication1.Application.Commands.Usuario.EditarDescricaoCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Descricao?.Length > 1000)
            return ResultadoService<int>.Falha("ProfileDescriptionTooLong");

        var descricao = string.IsNullOrWhiteSpace(command.Descricao) ? null : command.Descricao.Trim();
        return _usuarioRepository.AtualizarDescricao(id, descricao)
            ? ResultadoService<int>.Ok(id)
            : ResultadoService<int>.Falha("ProfileNotFound");
    }

    public Domain.Usuario.Usuario Buscar(int id)
    {
        return _usuarioRepository.BuscarPorId(id);
    }

}
