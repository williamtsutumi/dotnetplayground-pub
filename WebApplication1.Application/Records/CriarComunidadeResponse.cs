namespace WebApplication1.Application.Records;

public record CriarComunidadeResponse
{
    public int Id { get; set; }
    public string? UsuarioNome { get; set; }
    public int? UsuarioId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }
}
