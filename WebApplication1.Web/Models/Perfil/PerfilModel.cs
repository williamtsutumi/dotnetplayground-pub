namespace WebApplication1.Models.Perfil;

public class PerfilModel
{
    public int IdUsuario { get; set; }
    public bool ProprioPerfil { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public DateTime CriadoEm { get; set; }
    public string? Descricao { get; set; }

    public FeedModel? FeedModel { get; set; }
}
