using WebApplication1.Domain.Usuario.Enums;

namespace WebApplication1.Application.Records;

public record SalvarUsuarioRequest
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? Nome { get; set; }
    public string? Senha { get; set; }
    public string? Descricao { get; set; }
    public List<TipoPerfil> TiposPerfil { get; set; } = new List<TipoPerfil>();
}
