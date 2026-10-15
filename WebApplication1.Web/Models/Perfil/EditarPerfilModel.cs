namespace WebApplication1.Web.Models.Perfil;

public record EditarPerfilModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

