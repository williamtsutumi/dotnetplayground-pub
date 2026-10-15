namespace WebApplication1.Domain.Comunidade;

public class Comunidade
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int? IdCriadorUsuario { get; set; }
    public DateTime CriadoEm { get; set; }
}
